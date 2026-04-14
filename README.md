# LBH Housing Register Search Listener

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=LBHackney-IT_lbh-housing-register-search-listener&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=LBHackney-IT_lbh-housing-register-search-listener) [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=LBHackney-IT_lbh-housing-register-search-listener&metric=bugs)](https://sonarcloud.io/summary/new_code?id=LBHackney-IT_lbh-housing-register-search-listener) [![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=LBHackney-IT_lbh-housing-register-search-listener&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=LBHackney-IT_lbh-housing-register-search-listener) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=LBHackney-IT_lbh-housing-register-search-listener&metric=coverage)](https://sonarcloud.io/summary/new_code?id=LBHackney-IT_lbh-housing-register-search-listener) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=LBHackney-IT_lbh-housing-register-search-listener&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=LBHackney-IT_lbh-housing-register-search-listener) [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=LBHackney-IT_lbh-housing-register-search-listener&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=LBHackney-IT_lbh-housing-register-search-listener) [![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=LBHackney-IT_lbh-housing-register-search-listener&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=LBHackney-IT_lbh-housing-register-search-listener)

LBH Housing Register Search Listener is an AWS Lambda function that listens to SQS events triggered by changes to housing register applications. When a `HousingApplicationUpdatedEvent` is received, it retrieves the updated application from DynamoDB and indexes it into the search domain, keeping the housing register search index in sync with the latest application data.

## Stack

-   .NET 8 Core as the runtime framework.
-   xUnit as a test framework.

## Contributing

### Setup

1. Install [Docker][docker-download].
2. Install [AWS CLI][AWS-CLI].
3. Clone this repository.
4. Open it in your IDE.

### Development

To serve the application, run it using your IDE of choice, we use Visual Studio CE and JetBrains Rider on Mac.

**Note**
When running locally the appropriate database connection details are still needed.

### Pre-commit hooks

Repository has pre-commit hooks configuration to prevent direct commits to main branches and for scanning secrets. Please ensure you have [pre-commit framework](https://pre-commit.com/) installed before starting development work.

##### DynamoDb

To use a local instance of DynamoDb, this will need to be installed. This is most easily done using [Docker](https://www.docker.com/products/docker-desktop).
Run the following command, specifying the local path where you want the container's shared volume to be stored.

```
docker run --name dynamodb-local -p 8000:8000 -v <PUT YOUR LOCAL PATH HERE>:/data/ amazon/dynamodb-local -jar DynamoDBLocal.jar -sharedDb -dbPath /data
```

If you would like to see what is in your local DynamoDb instance using a simple gui, then [this admin tool](https://github.com/aaronshaf/dynamodb-admin) can do that.

The application can also be served locally using docker:

1.  Add your security credentials to AWS CLI.

```sh
$ aws configure
```

2. Log into AWS ECR.

```sh
$ aws ecr get-login --no-include-email
```

3. Build and run the application.

```sh
$ make build && make serve
```

### Release process

We use a pull request workflow, where changes are made on a branch and approved by one or more other maintainers before the developer can merge into `master` branch.

![Circle CI Workflow Example](docs/circle_ci_workflow.png)

Then we have an automated six step deployment process, which runs in CircleCI.

1. Automated tests (xUnit) are run to ensure the release is of good quality.
2. The application is deployed to development automatically, where we check our latest changes work well.
3. We manually confirm a staging deployment in the CircleCI workflow once we're happy with our changes in development.
4. The application is deployed to staging.
5. We manually confirm a production deployment in the CircleCI workflow once we're happy with our changes in staging.
6. The application is deployed to production.

Our staging and production environments are hosted by AWS. We deploy to production per each feature/config merged into `master` branch.

### Creating A PR

To help with making changes to code easier to understand when being reviewed, we've added a PR template.
When a new PR is created, the PR template will automatically fill in the `Open a pull request` description textbox.
The PR author can edit and change the PR description using the template as a guide.

## Static Code Analysis

### Using [FxCop Analysers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers)

FxCop runs code analysis when the Solution is built.

Both the listener and test projects have been set up to **treat all warnings from the code analysis as errors** and therefore, fail the build.

However, we can select which errors to suppress by setting the severity of the responsible rule to none, e.g `dotnet_analyzer_diagnostic.<Category-or-RuleId>.severity = none`, within the `.editorconfig` file.
Documentation on how to do this can be found [here](https://docs.microsoft.com/en-us/visualstudio/code-quality/use-roslyn-analyzers?view=vs-2019).

## Testing

### Run the tests

```sh
$ make test
```

### Agreed Testing Approach

-   Use xUnit, FluentAssertions and Moq
-   Always follow a TDD approach
-   Tests should be independent of each other
-   Gateway tests should interact with a real test instance of the database
-   Test coverage should never go down. (See the [test project readme](HousingRegisterSearchListener.Tests/readme.md#Run-coverage) for how to run a coverage check.)
-   All use cases should be covered by E2E tests
-   Optimise when test run speed starts to hinder development
-   Unit tests and E2E tests should run in CI
-   Test database schemas should match up with production database schema
-   Have integration tests which test from the DynamoDb database to the SQS message handler

## Data Migrations

### A good data migration

-   Record failure logs
-   Automated
-   Reliable
-   As close to real time as possible
-   Observable monitoring in place
-   Should not affect any existing databases

## Contacts

### Active Maintainers

-   **Selwyn Preston**, Head of Engineering at London Borough of Hackney (selwyn.preston@hackney.gov.uk)

[docker-download]: https://www.docker.com/products/docker-desktop
[AWS-CLI]: https://aws.amazon.com/cli/
