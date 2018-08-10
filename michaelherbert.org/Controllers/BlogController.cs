using michaelherbert.org.ViewModels;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace michaelherbert.org.Controllers
{
    

    public class BlogController : Controller
    {

        //public ActionResult Home1()
        //{
        //    return View();
        //}

        public int currentRow = 0;

        public ActionResult Home()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Home(BlogHomeViewModel vm)
        {
            string connStr = ConfigurationManager.ConnectionStrings["hetoshiCharityDBConnectionString"].ConnectionString;

            string oldNew = Request.QueryString["type"];
            currentRow = Convert.ToInt32(Request.QueryString["lastID"]);

            if (oldNew == "older")
            {
                currentRow += 1;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using(SqlCommand comm = new SqlCommand("SELECT * FROM (SELECT Id, Date, Blog_Post_Title, Blog_Post_Content, " +
                        "ROW_NUMBER() OVER (ORDER BY Id DESC) AS 'Row' FROM BlogPostTable) AS A WHERE Row BETWEEN " + currentRow + " AND " + (currentRow + 2), conn))

                    //    (" SELECT (TOP 3 ROW_NUMBER() OVER(ORDER BY Id DESC) AS Row, " +
                    //"Id, Date, Blog_Post_Title, Blog_Post_Content FROM BlogPostTable " +
                    //"WHERE Row BETWEEN " + currentRow + " AND " + (currentRow + 3), conn))

                    {
                        conn.Open();
                        using (SqlDataReader dr = comm.ExecuteReader())
                        {

                            string blogPostDate = null;
                            string blogPostTitle = null;
                            string blogPostContent = null;
                            int i = 0;

                            while (dr.Read())
                            {
                                blogPostDate = Convert.ToString(dr["Date"]);
                                blogPostTitle = Convert.ToString(dr["Blog_Post_Title"]);
                                blogPostContent = Convert.ToString(dr["Blog_Post_Content"]);
                                currentRow = Convert.ToInt32(dr["Row"]);
                                TempData["bpd" + i + ""] = blogPostDate;
                                TempData["bpt" + i + ""] = blogPostTitle;
                                TempData["bpc" + i + ""] = blogPostContent;
                                TempData["currentRow"] = currentRow;
                                //vm.BlogPostDate = blogPostDate;
                                //vm.BlogPostTitle = blogPostTitle;
                                //vm.BlogPostContent = blogPostContent;
                                i++;
                            }
                        }
                        conn.Close();
                    }
                }
            }
            else if (oldNew == "newer")
            {

                currentRow -= 2;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using (SqlCommand comm = new SqlCommand("SELECT * FROM (SELECT Id, Date, Blog_Post_Title, Blog_Post_Content, " +
                        "ROW_NUMBER() OVER (ORDER BY Id DESC) AS 'Row' FROM BlogPostTable) AS A WHERE Row BETWEEN " + (currentRow) + " AND " + (currentRow+2), conn))
                    {
                        conn.Open();
                        using (SqlDataReader dr = comm.ExecuteReader())
                        {

                            string blogPostDate = null;
                            string blogPostTitle = null;
                            string blogPostContent = null;
                            int i = 0;

                            while (dr.Read())
                            {
                                blogPostDate = Convert.ToString(dr["Date"]);
                                blogPostTitle = Convert.ToString(dr["Blog_Post_Title"]);
                                blogPostContent = Convert.ToString(dr["Blog_Post_Content"]);
                                currentRow = Convert.ToInt32(dr["Row"]);
                                TempData["bpd" + i + ""] = blogPostDate;
                                TempData["bpt" + i + ""] = blogPostTitle;
                                TempData["bpc" + i + ""] = blogPostContent;
                                TempData["currentRow"] = currentRow;
                                //vm.BlogPostDate = blogPostDate;
                                //vm.BlogPostTitle = blogPostTitle;
                                //vm.BlogPostContent = blogPostContent;
                                i++;
                            }
                        }
                        conn.Close();
                    }
                }
            }
            else
            { 
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using (SqlCommand comm = new SqlCommand("SELECT * FROM (SELECT Id, Date, Blog_Post_Title, Blog_Post_Content, " +
                        "ROW_NUMBER() OVER (ORDER BY Id DESC) AS 'Row' FROM BlogPostTable) AS A WHERE Row <= 3", conn))

                        //(" SELECT TOP 3 ROW_NUMBER() OVER(ORDER BY Id DESC) AS Row, " +
                        //"Id, Date, Blog_Post_Title, Blog_Post_Content FROM BlogPostTable", conn))                       
                    {
                        conn.Open();
                        using (SqlDataReader dr = comm.ExecuteReader())
                        {

                            string blogPostDate = null;
                            string blogPostTitle = null;
                            string blogPostContent = null;

                            int i = 0;

                            while (dr.Read())
                            {
                                blogPostDate = Convert.ToString(dr["Date"]);
                                blogPostTitle = Convert.ToString(dr["Blog_Post_Title"]);
                                blogPostContent = Convert.ToString(dr["Blog_Post_Content"]);
                                currentRow = Convert.ToInt32(dr["Row"]);
                                TempData["bpd" + i + ""] = blogPostDate;
                                TempData["bpt" + i + ""] = blogPostTitle;
                                TempData["bpc" + i + ""] = blogPostContent;
                                TempData["currentRow"] = currentRow;
                                //vm.BlogPostDate = blogPostDate;
                                //vm.BlogPostTitle = blogPostTitle;
                                //vm.BlogPostContent = blogPostContent;
                                i++;
                            }
                        }
                        conn.Close();
                    }

                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult Post()
        {
            

            string connStr = ConfigurationManager.ConnectionStrings["hetoshiCharityDBConnectionString"].ConnectionString;


            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand comm = new SqlCommand("SELECT TOP 1 * FROM BlogPostTable ORDER BY Id DESC", conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = comm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            TempData["latestBlogDate"] = Convert.ToString(dr["Date"]);
                            TempData["latestBlogTitle"] = Convert.ToString(dr["Blog_Post_Title"]);
                            TempData["latestBlogContent"] = Convert.ToString(dr["Blog_Post_Content"]);
                        }
                    }
                    conn.Close();
                }
            }

            return View();
        }

        [HttpPost]
        public ActionResult Post(string blogPostSubmit, BlogViewModel vm)
        {
            if (ModelState.IsValid)
            {

                int userID = 0;

                var utcTime = DateTime.UtcNow;
                DateTime mountainTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utcTime, "Mountain Standard Time");
                var blogPostDate = mountainTime;

                //try
                //{
                //    TimeZoneInfo mstZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                //    DateTime mstTime = TimeZoneInfo.ConvertTimeFromUtc(blogPostDate, mstZone);
                //}
                //catch (TimeZoneNotFoundException)
                //{

                //}
                //catch (InvalidTimeZoneException)
                //{

                //}
                
                var blogPostTitle = vm.Blog_Post_Title;
                var blogPostContent = vm.Blog_Post_Content;
                    
                string connStr = ConfigurationManager.ConnectionStrings["hetoshiCharityDBConnectionString"].ConnectionString;
                    
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using (SqlCommand comm = new SqlCommand("Insert_BlogPost", conn))
                    {
                        comm.CommandType = System.Data.CommandType.StoredProcedure;
                        comm.Parameters.AddWithValue("Date", blogPostDate);
                        comm.Parameters.AddWithValue("Blog_Post_Title", blogPostTitle);
                        comm.Parameters.AddWithValue("Blog_Post_Content", blogPostContent);

                        conn.Open();
                        userID = Convert.ToInt32(comm.ExecuteScalar());
                        comm.CommandTimeout = 4 * 10;
                        conn.Close();
                        
                    }
                }
                TempData["latestBlogDate"] = blogPostDate;
                TempData["latestBlogTitle"] = blogPostTitle;
                TempData["latestBlogContent"] = blogPostContent;
            }

            ModelState.Clear();
            return View();
        }

        public ActionResult readme()
        {
            return View();
        }
    }
    public class RichTextEditorViewModel
    {
        
    }
}
