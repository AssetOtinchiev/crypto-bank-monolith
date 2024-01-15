variable "name" {
  type = string
}

variable "network_id" {
  type = number
}

variable "private_ip" {
  type = string
}

variable "general_firewall_id" {
  type = number
}

variable "backend_ip" {
  type = string
}

variable "ssh_keys" {
  type = list(string)
}