using FinalProject.Areas.Admin.Models;
using FinalProject.Controllers;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FinalProject.Areas.Admin.Controllers
{
    public class BookController : Controller
    {
        // GET: Admin/Book
        public ActionResult Index()
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    return View(GetBooks());
                }
                else
                {
                    return Redirect("/Login?login=failed");
                }
            }
            else
            {
                return Redirect("/Login?login=loggedout");
            }
        }

        public List<Book> GetBooks()
        {
            int user_id = (int)Session["user_id"];
            List<Book> bookList = new List<Book>();
            ArrayList docIds = new ArrayList();
            MySqlConnection conn = HomeController.conn();
            MySqlConnection conn_inner = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            MySqlCommand inner_query_1 = conn.CreateCommand();
            MySqlCommand inner_query_2 = conn_inner.CreateCommand();
            query.CommandText = "SELECT branch_owner_user_id,doc_id FROM `branch` WHERE branch_owner_user_id = @user_id AND branch.is_deleted = 0";
            query.Parameters.AddWithValue("@user_id", user_id);
            try
            {
                conn.Open();
                Debug.WriteLine("query");
                MySqlDataReader fetchQuery = query.ExecuteReader();
                while (fetchQuery.Read())
                {
                    int doc_id = Convert.ToInt32(fetchQuery["doc_id"].ToString());
                    docIds.Add(doc_id);
                }
                conn.Close();
                foreach (var doc_id in docIds)
                {
                    conn.Open();
                    inner_query_1.CommandText = "SELECT subj_lang_level.subj_lang_level_id, document.doc_id, board.board_name, standard.standard, subject.subject, level.level, language.language_name, document.doc_title FROM ((((((`document` INNER JOIN subj_lang_level ON subj_lang_level.subj_lang_level_id = document.subj_lang_level_id) INNER JOIN language On language.language_id = subj_lang_level.language_id) INNER JOIN level On subj_lang_level.level_id = level.level_id) INNER JOIN subject ON subj_lang_level.subject_id = subject.subject_id) INNER JOIN standard ON standard.standard_id = subject.standard_id) INNER JOIN board ON board.board_id = standard.board_id) WHERE doc_id = @doc_id AND document.is_deleted = 0";
                    inner_query_1.Parameters.Clear();
                    inner_query_1.Parameters.AddWithValue("@doc_id", doc_id);
                    Debug.WriteLine("inner query 1");
                    MySqlDataReader innerFQ_1 = inner_query_1.ExecuteReader();

                    conn_inner.Open();
                    inner_query_2.CommandText = "SELECT document_versions.updated_at, user.user_fullname, document_versions.doc_version, document_versions.doc_subversion, document_versions.doc_revision_number FROM ((`branch` INNER JOIN document_versions ON branch.branch_id = document_versions.branch_id) INNER JOIN user ON user.user_id = branch.branch_owner_user_id)  WHERE doc_id=@doc_id AND document_versions.is_deleted = 0 ORDER BY document_versions.updated_at DESC LIMIT 1";
                    inner_query_2.Parameters.Clear();
                    inner_query_2.Parameters.AddWithValue("@doc_id", doc_id);
                    Debug.WriteLine("inner query 2");
                    MySqlDataReader innerFQ_2 = inner_query_2.ExecuteReader();

                    Debug.WriteLine("Finished all");
                    while (innerFQ_1.Read() && innerFQ_2.Read())
                    {
                        bookList.Add(new Book(
                            Convert.ToInt32(innerFQ_1["doc_id"].ToString()),
                            Convert.ToInt32(innerFQ_1["subj_lang_level_id"].ToString()),
                            innerFQ_1["doc_title"].ToString(),
                            innerFQ_1["board_name"].ToString(),
                            innerFQ_1["standard"].ToString(),
                            innerFQ_1["subject"].ToString(),
                            innerFQ_1["level"].ToString(),
                            innerFQ_1["language_name"].ToString(),
                            innerFQ_2["doc_version"].ToString() + "." + innerFQ_2["doc_subversion"].ToString() + "." + innerFQ_2["doc_revision_number"].ToString(),
                            innerFQ_2["updated_at"].ToString(),
                            innerFQ_2["user_fullname"].ToString()
                            )
                        );
                    }
                    conn.Close();
                    conn_inner.Close();
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException exception)
            {
                Debug.WriteLine(exception.ToString());
            }
            return bookList;
        }
        public ActionResult Add()
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    var tupleModel = new Tuple<List<Subject>, List<Level>, List<Language>>(GetSubject(), GetLevel(), GetLang());
                    return View(tupleModel);
                }
                else
                {
                    return Redirect("/Login?login=failed");
                }
            }
            else
            {
                return Redirect("/Login?login=loggedout");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(FormCollection fc)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    int sub_id = Convert.ToInt32(fc["sub_id"]);
                    int level_id = Convert.ToInt32(fc["level_id"]);
                    int lang_id = Convert.ToInt32(fc["lang_id"]);
                    string bookName = Convert.ToString(fc["book_name"]);
                    HttpFileCollectionBase file = Request.Files;
                    byte[] uploadfile = null;
                    if (file.Count == 1)
                    {
                        uploadfile = new byte[file[0].ContentLength];
                        file[0].InputStream.Read(uploadfile, 0, file[0].ContentLength);
                        Debug.WriteLine("Mila byte array \n" + uploadfile);
                    }

                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "INSERT INTO `subj_lang_level`(`subject_id`, `language_id`, `level_id`) VALUES(@sub_id,@lang_id,@level_id)";
                    query.Parameters.AddWithValue("@sub_id", sub_id);
                    query.Parameters.AddWithValue("@lang_id", lang_id);
                    query.Parameters.AddWithValue("@level_id", level_id);
                    int RowsAffected = -1;
                    try
                    {
                        conn.Open();
                        RowsAffected = query.ExecuteNonQuery();
                        long sll_id = query.LastInsertedId;
                        query.CommandText = "INSERT INTO `document`(`doc_title`, `subj_lang_level_id`, `created_by`) VALUES (@doc_title,@sll_id,@created_id)";
                        query.Parameters.AddWithValue("@doc_title", bookName);
                        query.Parameters.AddWithValue("@sll_id", sll_id);
                        query.Parameters.AddWithValue("@created_id", Session["user_id"]);
                        RowsAffected = query.ExecuteNonQuery();
                        long doc_id = query.LastInsertedId;
                        query.CommandText = "INSERT INTO `branch`(`doc_id`, `branch_owner_user_id`, `branch_name`, `is_merged`, `is_master_branch`, `created_by`) VALUES(@doc_id,@branch_owner_user_id,@branch_name,@is_merged,@is_master_branch,@created_by)";
                        query.Parameters.AddWithValue("@doc_id",doc_id);
                        query.Parameters.AddWithValue("@branch_owner_user_id", Session["user_id"]);
                        query.Parameters.AddWithValue("@branch_name","master");
                        query.Parameters.AddWithValue("@is_merged",0);
                        query.Parameters.AddWithValue("@is_master_branch",1);
                        query.Parameters.AddWithValue("@created_by",Session["user_id"]);
                        RowsAffected = query.ExecuteNonQuery();
                        long branch_id = query.LastInsertedId;
                        query.CommandText = "INSERT INTO `document_versions`(`branch_id`, `doc`, `doc_version`, `doc_subversion`, `doc_revision_number`, `doc_comment`, `doc_revision_date`, `created_by`) VALUES (@branch_id,@doc,@doc_version,@doc_subversion,@doc_revision_number,@doc_comment,@doc_revision_date,@created_byy)";
                        query.Parameters.AddWithValue("@branch_id", branch_id);
                        query.Parameters.AddWithValue("@doc",uploadfile);
                        query.Parameters.AddWithValue("@doc_version",0);
                        query.Parameters.AddWithValue("@doc_subversion", 0);
                        query.Parameters.AddWithValue("@doc_revision_number", 1);
                        query.Parameters.AddWithValue("@doc_comment","Initial Commit");
                        query.Parameters.AddWithValue("@doc_revision_date", DateTime.Now);
                        query.Parameters.AddWithValue("@created_byy", Session["user_id"]);
                        RowsAffected = query.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Exception " + e);
                    }
                    conn.Close();
                    return RedirectToAction("Index", "Book");
                }
                else
                {
                    return Redirect("/Login?login=failed");
                }
            }
            else
            {
                return Redirect("/Login?login=loggedout");
            }
        }
        public List<Subject> GetSubject()
        {
            List<Subject> sublist = new List<Subject>();
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT subject.subject_id, subject.subject, board.board_name, standard.standard FROM (`subject` INNER JOIN (standard INNER Join board On standard.board_id = board.board_id) ON standard.standard_id = subject.standard_id) WHERE subject.is_deleted = 0";
            MySqlDataReader fetchQuery;
            try
            {
                conn.Open();
                fetchQuery = query.ExecuteReader();
            }
            catch (Exception e)
            {
                fetchQuery = null;
                Debug.WriteLine("Exception " + e);
            }
            while (fetchQuery.Read())
            {
                sublist.Add(new Subject(Convert.ToInt32(fetchQuery["subject_id"].ToString()), fetchQuery["board_name"].ToString(), fetchQuery["standard"].ToString(), fetchQuery["subject"].ToString()));
            }
            conn.Close();
            return sublist;
        }

        public List<Level> GetLevel()
        {
            List<Level> levelList = new List<Level>();
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT `level_id`, `level` FROM `level`";
            MySqlDataReader fetchQuery;
            try
            {
                conn.Open();
                fetchQuery = query.ExecuteReader();
            }
            catch (Exception e)
            {
                fetchQuery = null;
                Debug.WriteLine("Exception " + e);
            }
            while (fetchQuery.Read())
            {
                levelList.Add(new Level(Convert.ToInt32(fetchQuery["level_id"].ToString()), fetchQuery["level"].ToString()));
            }
            conn.Close();
            return levelList;
        }

        public List<Language> GetLang()
        {
            List<Language> lagList = new List<Language>();
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT `language_id`, `language_name` FROM `language`";
            MySqlDataReader fetchQuery;
            try
            {
                conn.Open();
                fetchQuery = query.ExecuteReader();
            }
            catch (Exception e)
            {
                fetchQuery = null;
                Debug.WriteLine("Exception " + e);
            }
            while (fetchQuery.Read())
            {
                lagList.Add(new Language(Convert.ToInt32(fetchQuery["language_id"].ToString()), fetchQuery["language_name"].ToString()));
            }
            conn.Close();
            return lagList;
        }

        public ActionResult Edit(int id)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    var tupleModel = new Tuple<List<Subject>, List<Level>, List<Language>, Book>(GetSubject(), GetLevel(), GetLang(), GetBookOnId(id));
                    return View(tupleModel);
                }
                else
                {
                    return Redirect("/Login?login=failed");
                }
            }
            else
            {
                return Redirect("/Login?login=loggedout");
            }
        }

        public Book GetBookOnId(int doc_id)
        {
            Book book = null;
            MySqlConnection conn = HomeController.conn();
            MySqlConnection conn_inner = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            MySqlCommand inner_query_2 = conn_inner.CreateCommand();
            query.CommandText = "SELECT subj_lang_level.subj_lang_level_id, document.doc_id, board.board_name, standard.standard, subject.subject, level.level, language.language_name, document.doc_title FROM ((((((`document` INNER JOIN subj_lang_level ON subj_lang_level.subj_lang_level_id = document.subj_lang_level_id) INNER JOIN language On language.language_id = subj_lang_level.language_id) INNER JOIN level On subj_lang_level.level_id = level.level_id) INNER JOIN subject ON subj_lang_level.subject_id = subject.subject_id) INNER JOIN standard ON standard.standard_id = subject.standard_id) INNER JOIN board ON board.board_id = standard.board_id) WHERE doc_id = @doc_id AND document.is_deleted = 0";
            query.Parameters.AddWithValue("@doc_id", doc_id);
            try
            {
                conn.Open();
                Debug.WriteLine("query");
                MySqlDataReader fetchQuery = query.ExecuteReader();
                conn_inner.Open();
                inner_query_2.CommandText = "SELECT document_versions.updated_at, user.user_fullname, document_versions.doc_version, document_versions.doc_subversion, document_versions.doc_revision_number FROM ((`branch` INNER JOIN document_versions ON branch.branch_id = document_versions.branch_id) INNER JOIN user ON user.user_id = branch.branch_owner_user_id)  WHERE doc_id=@doc_id AND document_versions.is_deleted = 0 ORDER BY document_versions.updated_at DESC LIMIT 1";
                inner_query_2.Parameters.Clear();
                inner_query_2.Parameters.AddWithValue("@doc_id", doc_id);
                Debug.WriteLine("inner query 2");
                MySqlDataReader innerFQ_2 = inner_query_2.ExecuteReader();
                while (fetchQuery.Read() && innerFQ_2.Read())
                {
                    Debug.WriteLine(fetchQuery["subj_lang_level_id"].ToString());
                    book = new Book(
                        Convert.ToInt32(fetchQuery["doc_id"].ToString()),
                        Convert.ToInt32(fetchQuery["subj_lang_level_id"].ToString()),
                        fetchQuery["doc_title"].ToString(),
                        fetchQuery["board_name"].ToString(),
                        fetchQuery["standard"].ToString(),
                        fetchQuery["subject"].ToString(),
                        fetchQuery["level"].ToString(),
                        fetchQuery["language_name"].ToString(),
                        innerFQ_2["doc_version"].ToString() + "." + innerFQ_2["doc_subversion"].ToString() + "." + innerFQ_2["doc_revision_number"].ToString(),
                        innerFQ_2["updated_at"].ToString(),
                        innerFQ_2["user_fullname"].ToString()
                    );
                }
                conn.Close();
                conn_inner.Close();
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Exception - " + exc.ToString());
            }
            return book;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection fc)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    int sll_id = Convert.ToInt32(fc["sll_id"]);
                    int doc_id = Convert.ToInt32(fc["doc_id"]);
                    int sub_id = Convert.ToInt32(fc["sub_id"]);
                    int level_id = Convert.ToInt32(fc["level_id"]);
                    int lang_id = Convert.ToInt32(fc["lang_id"]);
                    string bookName = Convert.ToString(fc["book_name"]);

                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "UPDATE `subj_lang_level` SET `subject_id`=@sub_id, `language_id`=@lang_id ,`level_id`=@level_id WHERE `subj_lang_level_id`=@sll_id";
                    query.Parameters.AddWithValue("@sub_id", sub_id);
                    query.Parameters.AddWithValue("@lang_id", lang_id);
                    query.Parameters.AddWithValue("@level_id", level_id);
                    query.Parameters.AddWithValue("@sll_id", sll_id);
                    int RowsAffected = -1;
                    try
                    {
                        conn.Open();
                        RowsAffected = query.ExecuteNonQuery();
                        query.CommandText = "UPDATE `document` SET `doc_title`=@doc_title, `updated_by`=1 WHERE `doc_id`= @doc_id";
                        query.Parameters.AddWithValue("@doc_title", bookName);
                        query.Parameters.AddWithValue("@doc_id", doc_id);
                        RowsAffected = query.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Exception " + e);
                    }
                    conn.Close();
                    return RedirectToAction("Index", "Book");
                }
                else
                {
                    return Redirect("/Login?login=failed");
                }
            }
            else
            {
                return Redirect("/Login?login=loggedout");
            }
        }

        [HttpPost]
        public ActionResult Delete(int doc_id, int sll_id)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "UPDATE `document` SET `is_deleted`=1,`deleted_at`=@time,`deleted_by`=@user_id WHERE `doc_id`=@doc_id";
                    query.Parameters.AddWithValue("@time", DateTime.Now);
                    query.Parameters.AddWithValue("@user_id", (int)Session["user_id"]);
                    query.Parameters.AddWithValue("@doc_id", doc_id);
                    int RowsAffected = -1;
                    try
                    {
                        conn.Open();
                        RowsAffected = query.ExecuteNonQuery();
                        query.CommandText = "UPDATE `branch` SET `is_deleted`=1,`deleted_at`=@time,`deleted_by`=@user_id WHERE `branch_id` IN (SELECT branch_id FROM branch WHERE doc_id = @doc_id)";
                        query.Parameters.Clear();
                        query.Parameters.AddWithValue("@time", DateTime.Now);
                        query.Parameters.AddWithValue("@user_id", (int)Session["user_id"]);
                        query.Parameters.AddWithValue("@doc_id", doc_id);
                        RowsAffected = query.ExecuteNonQuery();

                        query.CommandText = "UPDATE `document_versions` SET `is_deleted`=1,`deleted_at`=@time,`deleted_by`=@user_id WHERE document_versions.branch_id IN (SELECT branch_id FROM branch WHERE branch.doc_id = @doc_id)";
                        query.Parameters.Clear();
                        query.Parameters.AddWithValue("@time", DateTime.Now);
                        query.Parameters.AddWithValue("@user_id", (int)Session["user_id"]);
                        query.Parameters.AddWithValue("@doc_id", doc_id);
                        RowsAffected = query.ExecuteNonQuery();
                        conn.Close();
                        return Json(new { result = "success" });
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                        return Json(new { result = "error" + e.ToString() });
                    }
                }
                else
                {
                    return Redirect("/Login?login=failed");
                }
            }
            else
            {
                return Redirect("/Login?login=loggedout");
            }
        }

        public ActionResult AssignCollaborator(int id)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    ViewBag.DocId = id;
                    return View(GetSubEditors(id));
                }
                else
                {
                    return Redirect("/Login?login=failed");
                }
            }
            else
            {
                return Redirect("/Login?login=loggedout");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignCollaborator(FormCollection fc)
        {
            int doc_id = Convert.ToInt32(fc["doc_id"]);
            string[] ids = fc["collab_id"].Split(',');
            foreach (var item in ids)
            {
                Debug.WriteLine(Convert.ToInt32(item));
                int user_id = Convert.ToInt32(item);
                string username = "";
                byte[] document = null;
                //Take each id
                //user_name = retrieve the username not fullname eg - jaynam, smeet28, etc
                MySqlConnection conn = HomeController.conn();
                MySqlCommand query = conn.CreateCommand();
                query.CommandText = "SELECT user.user_name FROM `user` WHERE user_id = @user_id";
                query.Parameters.Clear();
                query.Parameters.AddWithValue("@user_id", user_id);
                try
                {
                    conn.Open();
                    MySqlDataReader fetchQuery = query.ExecuteReader();
                    while (fetchQuery.Read())
                    {
                        username = fetchQuery["user_name"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("user exception " + ex);
                }
                conn.Close();
                //branch_id = retrieve the branch id based on doc_id & is_master = 1
                //file of master = retrieve the byte array of doc column in document_versions based on branch_id & order by doc_revision_date ASC
                MySqlCommand branch_query = conn.CreateCommand();
                branch_query.CommandText = "SELECT doc FROM document_versions WHERE branch_id = (SELECT branch_id FROM `branch` WHERE doc_id = @doc_id AND is_master_branch = 1) ORDER BY document_versions.doc_revision_date DESC";
                branch_query.Parameters.Clear();
                branch_query.Parameters.AddWithValue("@doc_id", doc_id);
                try
                {
                    conn.Open();
                    MySqlDataReader fetchQuery = branch_query.ExecuteReader();
                    while (fetchQuery.Read())
                    {
                        document = (byte[])fetchQuery["doc"];
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("document exception " + ex);
                }
                conn.Close();
                //insert into branch new record with branch_owner_user_id = user_id, branch_name = user_name, is_merged = 0, is_master_branch = 0
                //new_branch_id = get the last inserted branch id
                //insert a record in document_versions where doc = file of master, branch_id = new_branch_id, doc_version = 0.0.1, doc_comment = 'Assigned as Collaborator', doc_revision_date = Current_timestamp (should be included in query)
                MySqlCommand create_branch_query = conn.CreateCommand();
                MySqlCommand created_doc_ver = conn.CreateCommand();
                create_branch_query.CommandText = "INSERT INTO `branch`(`doc_id`, `branch_owner_user_id`, `branch_name`, `is_merged`, `is_master_branch`, `created_at`, `created_by`) VALUES (@doc_id,@branch_owner_user_id,@branch_name,@is_merged,@is_master_branch,@created_at,@created_by)";
                create_branch_query.Parameters.Clear();
                create_branch_query.Parameters.AddWithValue("@doc_id", doc_id);
                create_branch_query.Parameters.AddWithValue("@branch_owner_user_id", user_id);
                create_branch_query.Parameters.AddWithValue("@branch_name", username);
                create_branch_query.Parameters.AddWithValue("@is_merged", 0);
                create_branch_query.Parameters.AddWithValue("@is_master_branch", 0);
                create_branch_query.Parameters.AddWithValue("@created_at", DateTime.Now);
                create_branch_query.Parameters.AddWithValue("@created_by", Session["user_id"]);
                try
                {
                    conn.Open();
                    int RowsAffected = create_branch_query.ExecuteNonQuery();
                    long branch_id = create_branch_query.LastInsertedId;
                    Debug.WriteLine(branch_id.ToString() + " " + RowsAffected.ToString());
                    created_doc_ver.CommandText = "INSERT INTO `document_versions`(`branch_id`, `doc`, `doc_version`, `doc_subversion`, `doc_revision_number`, `doc_comment`, `doc_revision_date`, `created_at`, `created_by`) VALUES (@branch_id,@doc,@doc_version,@doc_subversion,@doc_revision_number,@doc_comment,@doc_revision_date,@created_at,@created_by)";
                    created_doc_ver.Parameters.AddWithValue("@branch_id", branch_id);
                    created_doc_ver.Parameters.AddWithValue("@doc", document);
                    created_doc_ver.Parameters.AddWithValue("@doc_version", 0);
                    created_doc_ver.Parameters.AddWithValue("@doc_subversion", 0);
                    created_doc_ver.Parameters.AddWithValue("@doc_revision_number", 1);
                    created_doc_ver.Parameters.AddWithValue("@doc_comment", "Colloboration Branch");
                    created_doc_ver.Parameters.AddWithValue("@doc_revision_date", DateTime.Now);
                    created_doc_ver.Parameters.AddWithValue("@created_at", DateTime.Now);
                    created_doc_ver.Parameters.AddWithValue("@created_by", Session["user_id"]);
                    RowsAffected = created_doc_ver.ExecuteNonQuery();
                    long doc_Ver_id = created_doc_ver.LastInsertedId;
                    Debug.WriteLine(doc_Ver_id.ToString() + " " + RowsAffected.ToString());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("branch insertion exception " + ex);
                }
                conn.Close();
            }
            return RedirectToAction("Index", "Book");
        }

        public List<SubEditor> GetSubEditors(int docid)
        {
            List<SubEditor> subEditors = new List<SubEditor>();
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT user_id, user_name, user_fullname FROM `user` WHERE user_role IN (SELECT user_role.role_id FROM user_role WHERE user_role.role = 'Sub Editor') AND user.is_deleted = 0 AND user.user_id NOT IN (SELECT branch_owner_user_id FROM `branch` WHERE doc_id = @doc_id)";
            query.Parameters.AddWithValue("doc_id", docid);
            MySqlDataReader fetchQuery;
            try
            {
                conn.Open();
                fetchQuery = query.ExecuteReader();
            }
            catch (Exception e)
            {
                fetchQuery = null;
                Debug.WriteLine("Exception " + e);
            }
            while (fetchQuery.Read())
            {
                subEditors.Add(new SubEditor(Convert.ToInt32(fetchQuery["user_id"].ToString()), fetchQuery["user_name"].ToString(), fetchQuery["user_fullname"].ToString()));
            }
            conn.Close();
            return subEditors;
        }

        public ActionResult DownloadFile(int id)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    Debug.WriteLine("I am here " + id);
                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    //query.CommandText = "SELECT * FROM `document_versions`,`document` WHERE branch_id IN(SELECT branch_id from branch where branch.doc_id = @doc_id and branch_owner_user_id = @user_id and is_deleted = 0) and document.doc_id = @doc_id ORDER BY document_versions.updated_at DESC";
                    query.CommandText = "SELECT * FROM `document_versions`,`document` WHERE branch_id IN (SELECT branch_id FROM branch WHERE doc_id = @doc_id AND is_deleted = 0 AND branch.is_master_branch = 1) ORDER BY document_versions.updated_at DESC";
                    query.Parameters.AddWithValue("@doc_id", id);
                    MySqlDataReader fetchQuery;
                    try
                    {
                        conn.Open();
                        fetchQuery = query.ExecuteReader();
                        Debug.WriteLine("Done");
                    }
                    catch (Exception e)
                    {
                        fetchQuery = null;
                        Debug.WriteLine("Exception " + e);
                    }
                    Debug.WriteLine("Query done");
                    byte[] data = null;
                    String title = "";
                    while (fetchQuery.Read())
                    {
                        title = fetchQuery["doc_title"].ToString();
                        data = (byte[])fetchQuery["doc"];
                    }
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.ContentType = "application/ms-word";
                    Response.AddHeader("content-disposition", "attachment;filename=" + title + ".docx");
                    Response.BinaryWrite(data);
                    Response.Flush();
                    Response.End();
                    Debug.WriteLine("All done");
                    return new EmptyResult();
                }
                else
                {
                    return Redirect("/Login?login=failed");
                }
            }
            else
            {
                return Redirect("/Login?login=loggedout");
            }
        }
    }
}