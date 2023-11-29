namespace PustokHomeWork.Areas.Admin.Service
{
    public class FileManager
    {




        public static string Save(IFormFile file)
        {
            string filename = Guid.NewGuid().ToString() + new FileInfo(file.FileName).Extension;
            FileStream str = new FileStream("C:\\Users\\Es\\source\\repos\\HomeWork\\FileUpload\\AllUp\\AllUpHomeWork\\wwwroot\\product_images\\" + filename, FileMode.Create);
            file.CopyTo(str);

            return filename;
        }

        public static void Remove(string filename)
        {
            File.Delete("C:\\Users\\Es\\source\\repos\\HomeWork\\FileUpload\\AllUp\\AllUpHomeWork\\wwwroot\\product_images\\" + filename);
        }
    }

  
}
