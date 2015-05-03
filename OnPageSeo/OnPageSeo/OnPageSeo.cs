using System;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.HtmlControls;
using BlogEngine.Core;
using BlogEngine.Core.Web.Controls;

namespace BlogEngine.NET.Custom.Extensions
{

    [Extension("OnPage Seo with Google Author and Open Graph protocol, Linked In , Facebook, Twitter , Google Plus ", "3.1.0.1", "Adrian Cini")]
    public class OnPageSeo
    {
        public OnPageSeo()
        {
            Post.Serving += new EventHandler<ServingEventArgs>(Post_Serving);
        }

        private string GoogleAuthorId
        {
            get
            {
                //To show author details you need to add a web.config <appSettings> section
                //<add key ="GoogleAuthorId" value="111254170622220751214"/>
                //where the value is your google plus profile id
                var googleAuthorId = ConfigurationManager.AppSettings["GoogleAuthorId"];
                if (!string.IsNullOrEmpty(googleAuthorId))
                {
                    return googleAuthorId;
                }
                return string.Empty;
            }
        }

        public void Post_Serving(object sender, ServingEventArgs e)
        {
            //during the loading of a full post
            if (e.Location == ServingLocation.SinglePost)
            {
                //On page seo for posts only
                var post = sender as BlogEngine.Core.Post;
                if (post != null)
                {

                    HttpContext context = HttpContext.Current;
                    if (context != null)
                    {
                        System.Web.UI.Page page = context.CurrentHandler as System.Web.UI.Page;
                        if (page != null)
                        {
                            //Add ogg schema to the page html header [The Open Graph protocol]

                            //Important add: <html lang="en" prefix="og: http://ogp.me/ns#"> in the template/xxx/site.master 
                            HtmlMeta OgTitleTag = new HtmlMeta();
                            OgTitleTag.Attributes.Add("Property", "og:title");
                            OgTitleTag.Attributes.Add("content", post.Title);
                            page.Header.Controls.Add(OgTitleTag);


                            HtmlMeta OgDescTag = new HtmlMeta();
                            OgDescTag.Attributes.Add("Property", "og:description");
                            OgDescTag.Attributes.Add("content", post.Description);
                            page.Header.Controls.Add(OgDescTag);

                            HtmlMeta OgUrlTag = new HtmlMeta();
                            OgUrlTag.Attributes.Add("Property", "og:url");
                            OgUrlTag.Attributes.Add("content", post.AbsoluteLink.ToString());
                            page.Header.Controls.Add(OgUrlTag);

                            HtmlMeta OgImageTag = new HtmlMeta();
                            OgImageTag.Attributes.Add("Property", "og:image");
                            OgImageTag.Attributes.Add("content", GetFirstImageInArticle(post));
                            page.Header.Controls.Add(OgImageTag);


                        }
                    }

                    // SetOGSchema(post);

                    StringBuilder authorDiv = new StringBuilder();
                    authorDiv.AppendLine("<div class=\"well\" >");
                    authorDiv.AppendLine("<div class=\"row\" >");
                    if (!string.IsNullOrEmpty(GoogleAuthorId))
                    {
                        authorDiv.AppendLine("<div class=\"col-md-12\" ><h3>About the author</h3></div>");
                        authorDiv.AppendLine(
                            string.Format("<div class=\"col-md-2\">{0}</div><div class=\"col-md-10\">{1}{2}</div>",
                                GetAuthorTag(GoogleAuthorId), post.AuthorProfile.AboutMe, TwitterFollow()));
                    }
                    authorDiv.AppendLine(string.Format("<div class=\"col-md-3\">{0}</div><div class=\"col-md-3\">{1}</div><div class=\"col-md-3\">{2}</div><div class=\"col-md-3\">{3}</div>", FaceBookShare(),
                        LinkedInShare(post.AbsoluteLink.ToString()), GooglePlusShare(post.AbsoluteLink.ToString()), TwitterShare()));
                    authorDiv.AppendLine("</div></div>");
                    e.Body += authorDiv.ToString();

                }
            }
        }

        private string GetFirstImageInArticle(BlogEngine.Core.Post thisPost)
        {
            var htmlString = thisPost.Content;
            //<\s*img [^\>]*src\s*=\s*(["\'])(.*?)\1
            string pattern = @"<\s*img [^\>]*src\s*=\s*([";
            pattern += "\"\\'])(.*?)\\1";

            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(htmlString);
            if (matches.Count > 0)
            {
                var firstImage = matches[0].ToString();
                string[] tmp = firstImage.Split('"');
                string relPath = tmp[1].Trim();
                string absPath = HttpContext.Current.Server.MapPath(relPath);
                return absPath;
            }
            return "";


        }

        private string GetAuthorTag(string authorId)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("<a rel=\"author\" href=\"https://plus.google.com/{0}?rel=author\" target=\"_blank\">", authorId));
            sb.AppendLine(string.Format("<img src=\"https://lh5.googleusercontent.com/-OdgyqYc5Gs0/AAAAAAAAAAI/AAAAAAAADbM/0KsoblqPcco/s46-c-k-no/photo.jpg\" width=\"46px\" height=\"46px\" alt=\"Adrian Cini\" class=\"Uk wi hE\" oid=\"{0}\"></a>", authorId));
            return sb.ToString();
        }

        private string FaceBookShare()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<script>function fbs_click() {u=location.href;t=document.title;window.open('http://www.facebook.com/sharer.php?u='+encodeURIComponent(u)+'&t='+encodeURIComponent(t),'sharer','toolbar=0,status=0,width=626,height=436');");
            sb.AppendLine(
                "return false;}</script><style> html .fb_share_link { padding:2px 0 0 20px; height:16px; background:url(http://static.ak.facebook.com/images/share/facebook_share_icon.gif?6:26981) no-repeat top left; }</style><a rel=\"nofollow\" href=\"http://www.facebook.com/share.php?u=@Request.Url.GetLeftPart(UriPartial.Authority)/FullArticle/@Model.ArticleId/@Model.Title\" onclick=\"return fbs_click()\" target=\"_blank\" class=\"fb_share_link\">Facebook</a>");
            return sb.ToString();
        }

        private string TwitterFollow()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("<a href=\"{0}\" class=\"twitter-follow-button\" data-show-count=\"false\">{1}</a>", "https://twitter.com/drinu82", "Follow @Adrian Cini"));
            sb.AppendLine(
                "<script>!function(d,s,id){var js,fjs=d.getElementsByTagName(s)[0],p=/^http:/.test(d.location)?'http':'https';if(!d.getElementById(id)){js=d.createElement(s);js.id=id;js.src=p+'://platform.twitter.com/widgets.js';fjs.parentNode.insertBefore(js,fjs);}}(document, 'script', 'twitter-wjs');</script>");
            return sb.ToString();
        }

        private string TwitterShare()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<a href=\"https://twitter.com/share\" class=\"twitter-share-button\" data-via=\"drinu82\">Tweet</a>");
            sb.AppendLine(
                "<script>!function(d,s,id){var js,fjs=d.getElementsByTagName(s)[0],p=/^http:/.test(d.location)?'http':'https';if(!d.getElementById(id)){js=d.createElement(s);js.id=id;js.src=p+'://platform.twitter.com/widgets.js';fjs.parentNode.insertBefore(js,fjs);}}(document, 'script', 'twitter-wjs');</script>");
            return sb.ToString();
        }

        private string LinkedInShare(string url)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<script src=\"//platform.linkedin.com/in.js\" type=\"text/javascript\"> lang: en_US</script>");
            sb.AppendLine(string.Format(
                "<script type=\"IN/Share\" data-url=\"{0}\" data-counter=\"right\"></script>", url));
            return sb.ToString();
        }

        private string GooglePlusShare(string url)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<script src=\"https://apis.google.com/js/platform.js\" async defer></script>");
            sb.AppendLine("<div class=\"g-plusone\" data-annotation=\"inline\" data-width=\"300\"></div>");
            return sb.ToString();
        }


    }
}