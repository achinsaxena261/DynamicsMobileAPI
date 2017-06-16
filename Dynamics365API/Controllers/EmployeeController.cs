using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Xrm.Sdk;
using System.Configuration;
using earlybound;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Query;
using Dynamics365API.Models;
using Newtonsoft.Json;
using System.Text;

namespace Dynamics365API.Controllers
{
    public class EmployeeController : ApiController
    {
        public HttpResponseMessage GetAllEmployees()
        {
            try
            {
                // Get the CRM connection string and connect to the CRM Organization
                List<Employee> _newEmployees = new List<Employee>();
                using (CrmServiceClient crmConn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM"].ConnectionString))
                {
                    IOrganizationService orgSvc = crmConn.OrganizationServiceProxy;
                    QueryExpression _sdkQuery = new QueryExpression
                    {
                        EntityName = "new_employee",
                        //ColumnSet = new ColumnSet("new_employeeid", "new_legalname", "new_alternateemail", "new_organization"),
                        ColumnSet = new ColumnSet("new_employeeid", "new_legalname")
                    };
                    EntityCollection employees = orgSvc.RetrieveMultiple(_sdkQuery);
                    foreach(Entity employee in employees.Entities)
                    {
                        _newEmployees.Add(new Employee
                        {
                            Id = employee.Attributes["new_employeeid"].ToString(),
                            Name = employee.Attributes["new_legalname"].ToString(),
                            //Email = employee.Attributes["new_alternateemail"].ToString(),
                            //Organization = employee.Attributes["new_organization"].ToString()
                        });
                    }
                }
               return Request.CreateResponse(HttpStatusCode.OK,_newEmployees);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest,ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage ValidateEmployees([FromUri]Employee employee)
        {
            try
            {
                // Get the CRM connection string and connect to the CRM Organization
                using (CrmServiceClient crmConn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM"].ConnectionString))
                {
                    IOrganizationService orgSvc = crmConn.OrganizationServiceProxy;
                    QueryExpression _sdkQuery = new QueryExpression
                    {
                        EntityName = "new_employee",
                        ColumnSet = new ColumnSet("new_name", "new_legalname", "new_alternateemail", "new_organization"),
                        Criteria = new FilterExpression(LogicalOperator.And)
                    };
                    _sdkQuery.Criteria.AddCondition(_sdkQuery.EntityName, "new_name", ConditionOperator.Equal, employee.Id);
                    _sdkQuery.Criteria.AddCondition(_sdkQuery.EntityName, "new_password", ConditionOperator.Equal, employee.Password);
                    EntityCollection employees = orgSvc.RetrieveMultiple(_sdkQuery);
                    if(employees.Entities.Count > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "success");
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK,"Failed");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        [HttpGet]
        public HttpResponseMessage GetEmployeeDetails(string Id)
        {
            try
            {
                using (CrmServiceClient crmConn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM"].ConnectionString))
                {
                    IOrganizationService _orgService = crmConn.OrganizationServiceProxy;
                    QueryExpression query = new QueryExpression("new_employee");
                    string[] cols = { "new_employeeid", "new_name" };
                    query.Criteria = new FilterExpression();
                    query.Criteria.AddCondition("new_name", ConditionOperator.Equal, Id);
                    query.ColumnSet = new ColumnSet(cols);
                    var account = _orgService.RetrieveMultiple(query);
                    var details = _orgService.Retrieve("new_employee", (Guid)account[0].Attributes["new_employeeid"], new ColumnSet(true));
                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent(JsonConvert.SerializeObject(details), Encoding.UTF8, "application/json");
                    return response;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

    }
}
