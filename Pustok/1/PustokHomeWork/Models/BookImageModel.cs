namespace PustokHomeWork.Models
{
    public class BookImageModel
    {
        public int Id { get; set; }
        public string ImageURI { get; set; }
        public bool? isPoster { get; set; }

        public int BookId { get; set; }
        public BookModel Book { get; set; }

    }
}
