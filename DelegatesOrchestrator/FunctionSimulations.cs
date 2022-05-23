using DelegatesOrchestrator.Types;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DelegatesOrchestrator
{
    public static class FunctionSimulations
    {
        public static void WorkWithExceptionParameterless()
        {
            Thread.Sleep(3000);
            throw new Exception("WorkWithExceptionParameterless failed !");
        }

        public static void WorkWithException(WrapperRequest request)
        {
            Thread.Sleep(3000);
            throw new ArgumentException("You request is not in valid format");
        }

        public static WrapperResponse WorkWithException()
        {
            Thread.Sleep(3000);
            throw new Exception("Application lost connection");
        }

        public static void Work1InVoid(WrapperRequest request)
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} started working"
                + Environment.NewLine +
                " on WrapperRequest with void output");
            Thread.Sleep(3000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished");
        }

        public static WrapperResponse Work1OutParameterless()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} started working"
                + Environment.NewLine +
                " parameterless with WrapperResponse");
            Thread.Sleep(3000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished");
            return new WrapperResponse { result = "{\"property1\":\"30\",\"property2\":\"12\"}" };
        }

        public static WrapperResponse Work1In1Out(WrapperRequest request)
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} started working"
                + Environment.NewLine +
                " on WrapperRequest with WrapperResponse");
            Thread.Sleep(3000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished");
            return new WrapperResponse { result = "{\"property1\":\"10\",\"property2\":\"15\"}" };
        }

        public static void WorkVoidParameterLess()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} started working.");
            Thread.Sleep(3000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished working.");
        }

        public static void DbDummyFetch()
        {
            var con = new SqlConnection("Data Source=GRPC003202;Initial Catalog=Employee;Integrated Security=True");
            con.Open();

            SqlDataReader result;
            result = new SqlCommand("select * from dbo.Employees", con).ExecuteReader();

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
