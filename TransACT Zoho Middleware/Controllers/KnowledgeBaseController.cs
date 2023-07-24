using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.AspNetCore.Webhooks.Models;
using Kontent.Ai.Urls.Delivery.QueryParameters;
using TransACT_Zoho_Middleware.Models;
using KontentAiModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TransACT_Zoho_Middleware.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KnowledgeBaseController : ControllerBase
    {

        private IDeliveryClient _deliveryClient;

        

        public KnowledgeBaseController(IDeliveryClient deliveryClient)
        {
            _deliveryClient = deliveryClient;
        }

        private string AuthToken()
        {
            // Replace with 1 hour refresh token
            // replace with cron job or 1 hour refresh code - hardcoded for demo.
            string TOKEN = "1000.a3ee20efd58ece85f9fbdf89297df191.b08a4e7bea36d32e0169fee0579fd7b6";
            return TOKEN;
        }

        private async Task<KnowledgeBaseArticle> getKontentItem(string codename)
        {
            try
            {
                var response = await _deliveryClient.GetItemAsync<KnowledgeBaseArticle>(
                    codename,
                    new DepthParameter(3)
                    );

                KnowledgeBaseArticle item = response.Item;

                return item;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
        [HttpGet("{codename}")]
        public async Task<ActionResult<KnowledgeBaseArticle>> GetZohoArticle(string codename)
        {
            return Ok();
        }

        // POST api/<ValuesController>
        [HttpPost("PostArticle/{codename}")]

        public async Task<ActionResult<ZohoArticle>> PostArticle([FromBody] DeliveryWebhookModel model)
        {
            try
            {
                var kontentArticle = await getKontentItem(model.Data.Items.First().Codename);// replace with incoming value

                string zohoApiUrl = "https://desk.zoho.com/api/v1/articles?orgId=819166171";

                var zohoArticle = new ZohoArticle();

                long authorId = long.Parse(kontentArticle.Author.FirstOrDefault().AuthorId);
                long categoryId = long.Parse(kontentArticle.Category.FirstOrDefault().CategoryId);

                zohoArticle.title = kontentArticle.Title;
                zohoArticle.permission = kontentArticle.Permission.FirstOrDefault().Name;
                zohoArticle.answer = kontentArticle.Answer;
                zohoArticle.status = kontentArticle.Status.FirstOrDefault().Name;
                zohoArticle.categoryId = categoryId;
                zohoArticle.authorId = authorId;



                using (var client = new HttpClient())
                {
                    var AccessToken = AuthToken();
                    client.DefaultRequestHeaders.Add("Authorization", "Zoho-oauthtoken " + AccessToken);

                    // Set the Content-Type header to specify JSON format
                    client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

                    var zohoJSON = JsonConvert.SerializeObject(zohoArticle);
                    var stringContent = new StringContent(zohoJSON, UnicodeEncoding.UTF8,
                                    "application/json");
                    
                    
                    var response = await client.PostAsync(zohoApiUrl, stringContent);

                    return Ok(response);
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
