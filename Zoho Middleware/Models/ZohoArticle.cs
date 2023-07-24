namespace TransACT_Zoho_Middleware.Models
{
    public class ZohoArticle
    {
        //  "title"
        //  "permission" : "ALL",
        //  "answer" : "The knowledge base text...",
        //  "categoryId" : 22372000000094160,
        //  "status" : "Draft"

        public string title { get; set; }
        public string permission { get; set; }
        public string answer { get; set; }
        public long categoryId { get; set; }
        public long authorId { get; set; }
        public string status { get; set; }
    }
}
