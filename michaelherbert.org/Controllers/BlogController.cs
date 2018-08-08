﻿using michaelherbert.org.ViewModels;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;


namespace michaelherbert.org.Controllers
{
    
    
    public class BlogController : Controller
    {
        
        //public ActionResult Home1()
        //{
        //    return View();
        //}
        
        public ActionResult Home()
        {
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
        public ActionResult Post(BlogViewModel vm)
        {
            if (ModelState.IsValid)
            {

                int userID = 0;
                
                var blogPostDate = DateTime.Now;
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
            return View();
        }

        public ActionResult readme()
        {
            return View();
        }
    }
}
