# Kumorai Test Project

## Author: Mohammad Hassan Aamir

## Overview
Test project for Kumurai contains Azure Functions to retrieve billing information for Azure resources.

## Prerequisites
- .NET SDK
- Azure Functions Core Tools
- Azure Account
- Azure SDK for .NET

## Http Trigger Functions
1. `GetBillingInfoByResourceId`
2. `GetBillingInfoByResourceIds`
3. `GetBillingInfoBySubscriptionId`

## Setup and Run
Code:
1. Clone the repository.
2. Navigate to the project directory.
3. Add service principal credentials in local.settings.json (see Azure)
4. Install the required packages:
   dotnet restore
5. Run Project
6. Open Postman and test collection :
   https://grey-equinox-148404.postman.co/workspace/New-Team-Workspace~800010db-0459-45a1-be76-782aae162254/collection/26824320-a3212bc8-2f9c-4818-81a9-b26783b77f3d?action=share&creator=26824320

Azure:
1. Go to portal
2. Open Azure CLI
3. Create service principal using : 
   az ad sp create-for-rbac --name servicePrincipalName --role {role} --scopes {scope}
4. Go to App regestration and assign azure mangement api permission
5. Go to Cost management and assign IAM role to service principal
