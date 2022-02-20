using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Multithreading
{
    public class dbLayer
    {
        public static void Db()
       {
           var con = new SqlConnection("Data Source=GRPC003202;Initial Catalog=Employee;Integrated Security=True");
           con.Open();

           SqlDataReader result;
           result = new SqlCommand("select * from dbo.Employees",con).ExecuteReader();

           if (result.HasRows)
           {
               while (result.Read())
               {
                   Console.WriteLine("Name : " + result.GetString(1) + "Profession : " + result.GetString(2));
               }

           }
           else
               Console.WriteLine("No rows fetched");
           result.Close();
           con.Close();
       }
    }
}
