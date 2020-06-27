using System.Web;
using System.Web.Optimization;

namespace FinalProject
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/basecss").Include(
                      "~/Content/assets/css/bootstrap.min.css",
                      "~/Content/assets/css/style.css", 
                      "~/Content/assets/css/components.css",
                      "~/Content/assets/css/custom.css"));

            bundles.Add(new StyleBundle("~/Content/icons").Include(
                "~/Content/assets/css/all.min.css",
                "~/Content/assets/css/ionicons.min.css"));

            bundles.Add(new StyleBundle("~/Content/dropdowns").Include(
                "~/Content/assets/css/select2.min.css",
                "~/Content/assets/css/selectric.css"));

            bundles.Add(new StyleBundle("~/Content/socialcss").Include("~/Content/assets/css/bootstrap-social.css"));

            bundles.Add(new StyleBundle("~/Content/toastrcss").Include("~/Content/assets/css/iziToast.min.css"));

            bundles.Add(new ScriptBundle("~/Content/basejs").Include(
                        "~/Content/assets/js/jquery-3.3.1.min.js",
                        "~/Content/assets/js/popper.min.js",
                        "~/Content/assets/js/bootstrap.min.js",
                        "~/Content/assets/js/jquery.nicescroll.min.js",
                        "~/Content/assets/js/moment.min.js",
                        "~/Content/assets/js/stisla.js",
                        "~/Content/assets/js/scripts.js",
                        "~/Content/assets/js/jquery.sticky-kit.js"));

            bundles.Add(new ScriptBundle("~/Content/templatejs").Include("~/Content/assets/js/custom.js"));

            bundles.Add(new ScriptBundle("~/Content/vendorjs").Include(
                "~/Content/assets/js/vendor/jquery.selectric.min.js",
                "~/Content/assets/js/vendor/select2.full.min.js",
                "~/Content/assets/js/vendor/iziToast.min.js",
                "~/Content/assets/js/vendor/chart.min.js"
                ));

            bundles.Add(new ScriptBundle("~/Content/register").Include("~/Content/assets/js/page/auth-register.js", "~/Content/assets/js/jquery.pwstrength.js"));

            bundles.Add(new ScriptBundle("~/Content/modal").Include("~/Content/assets/js/page/bootstrap-modal.js"));

            bundles.Add(new ScriptBundle("~/Content/toastr").Include("~/Content/assets/js/page/modules-toastr.js"));

            bundles.Add(new ScriptBundle("~/Content/internal-pages").IncludeDirectory("~/Content/assets/js/internal-pages","*.js",true));

            bundles.Add(new ScriptBundle("~/Content/internal-pages/render-admin-commits").Include("~/Content/assets/js/internal-pages/render-admin-commits.js"));

        }
    }
}
