using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Glue.AzdoAuthentication;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.Build.WebApi;

namespace app
{
    [Route("api/run-setup")]
    [ApiController]
    public class RunSetupController : ControllerBase
    {
        private readonly IMediator mediator;

        public RunSetupController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<Build>> Post(RunSetup request)
        {
            Build result = await mediator.Send(request);
            return result;
        }
    }

    public class SetupSettings
    {
        public string RepositoryId { get; set; }
        public string YamlFilename { get; set; } = "azure-release-pipelines.yml";
        public string ProjectName { get; set; }
        public int? AgentPoolQueueId { get; set; }
        public int? DefinitionToCloneId { get; set; }
    }

    public class RunSetup : IRequest<Build>
    {
        public Dictionary<string, string> Parameters { get; set; }
    }

    public class RunSetupHandler : IRequestHandler<RunSetup, Build>
    {
        private readonly AzdoClients clients;
        private readonly SetupSettings settings;

        public RunSetupHandler(AzdoClients clients, SetupSettings settings)
        {
            this.clients = clients;
            this.settings = settings;
        }

        public async Task<Build> Handle(RunSetup request, CancellationToken cancellationToken)
        {
            BuildHttpClient c = await clients.GetAppClient<BuildHttpClient>();
            BuildDefinition definition = new BuildDefinition
            {
                Name = "new name" + Guid.NewGuid().ToString(),
                Repository = new BuildRepository
                {
                    Id = settings.RepositoryId,
                    Type = "TfsGit"
                },
                Process = new YamlProcess
                {
                    YamlFilename = settings.YamlFilename
                },
                Type = DefinitionType.Build,
                Queue = new AgentPoolQueue { Id = settings.AgentPoolQueueId.Value },
            };
            foreach ((string key, string value) in request.Parameters)
            {
                definition.Variables.Add(key, new BuildDefinitionVariable
                {
                    Value = value,
                    AllowOverride = false
                });
            }

            BuildDefinition result = await c.CreateDefinitionAsync(
                definition: definition,
                definitionToCloneId: settings.DefinitionToCloneId,
                project: settings.ProjectName);
            Build build = await c.QueueBuildAsync(
                build: new Build { Definition = new DefinitionReference { Id = result.Id } },
                project: settings.ProjectName);

            return build;
        }
    }
}
