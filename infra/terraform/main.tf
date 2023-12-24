terraform {
  required_providers {
    azurerm = {
      source = "hashicorp/azurerm"
      version = "3.85.0"
    }
  }
}

provider "azurerm" {
  subscription_id = ""
  client_id = ""
  client_secret = ""
  tenant_id = ""
  features {}
}

resource "azurerm_resource_group" "app_grp" {
  name = "app-grp"
  location = "North Europe"
}