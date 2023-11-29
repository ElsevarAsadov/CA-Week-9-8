using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PustokHomeWork.Models
{
    public class BookModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }

        public int AuthorId { get; set; }

        public AuthorModel? Author { get; set; }

        public List<TagModel>? Tags { get; set; }

        [NotMapped]
        public List<int> TagIds { get; set; }

        public List<BookImageModel>? BookImages { get; set; }

        [NotMapped]
        public List<IFormFile>? ImageFiles { get; set; }

        [NotMapped]
        public IFormFile? BookPosterImage { get; set; }
        [NotMapped]
        public IFormFile? BookHoverImage { get; set; }





    }
}
