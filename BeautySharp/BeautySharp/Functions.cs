using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;

namespace BeautySharp
{
    public class Functions
    {
        public static string GetDocumentText(Document document)
        {
            var textDocument = (TextDocument)document.Object("TextDocument");
            EditPoint editPoint = textDocument.StartPoint.CreateEditPoint();
            var content = editPoint.GetText(textDocument.EndPoint);
            return content;
        }


        public static string WebPost(string url, string postData)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            byte[] data = Encoding.ASCII.GetBytes(postData);
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            switch (responseString)
            {
                case "Wrong request.":
                    MessageBox.Show("Internal error. Please report this to the developer.");
                    return "";
                case "DB Error.":
                    MessageBox.Show("Server not working currently.");
                    return "";
                case "Error creating file.":
                    MessageBox.Show("Could not paste. Make sure a valid file is open.");
                    return "";
            }

            return responseString;
        }
    }
}
