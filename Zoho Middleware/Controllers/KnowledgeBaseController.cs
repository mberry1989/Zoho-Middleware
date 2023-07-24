using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.AspNetCore.Webhooks.Models;
using Kontent.Ai.Urls.Delivery.QueryParameters;
using Zoho_Middleware.Models;
using KontentAiModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Zoho_Middleware.Controllers
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
            // use https://api-console.zoho.com/  for creating tokens
            // helpful instructions for manual tokens:
            // https://help.zoho.com/portal/en/community/topic/connecting-to-zoho-desk-api-with-postman
            // replace this with 1 hour refresh schedule task code in a long-term solution.
            string TOKEN = "1000.56b258b0838aa0b3a4a6ce894479395e.2ef97a8412ee054380d35122482920e2";
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

        // POST api/KnowledgeBase/PostArticle
        [HttpPost("PostArticle")]

        public async Task<ActionResult<ZohoArticle>> PostArticle([FromBody] DeliveryWebhookModel model)
        {
            try
            {
                var kontentArticle = await getKontentItem(model.Data.Items.First().Codename);

                // Get orgId from Zoho
                string zohoApiUrl = "https://desk.zoho.com/api/v1/articles?orgId=819166171";

                // Format Kontent.ai content to Zoho required API values
                var zohoArticle = new ZohoArticle();

                long authorId = long.Parse(kontentArticle.Author.FirstOrDefault().AuthorId);
                long categoryId = long.Parse(kontentArticle.Category.FirstOrDefault().CategoryId);

                zohoArticle.title = kontentArticle.Title;
                zohoArticle.permission = kontentArticle.Permission.FirstOrDefault().Name;
                zohoArticle.answer = kontentArticle.Answer;
                zohoArticle.status = kontentArticle.Status.FirstOrDefault().Name;
                zohoArticle.categoryId = categoryId;
                zohoArticle.authorId = authorId;


                // Zoho API POST
                using (var client = new HttpClient())
                {
                    var AccessToken = AuthToken();
                    client.DefaultRequestHeaders.Add("Authorization", "Zoho-oauthtoken " + AccessToken);

                    // Set the Content-Type header to specify JSON format
                    client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

                    // Format new Zoho object to JSON readable by endpoint
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
