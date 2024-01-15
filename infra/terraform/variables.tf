variable "hcloud_token" {
  type = string
  sensitive = true
}

variable "ssh_key_fingerprint" {
  type = string
  default = "36:35:a6:14:5c:d8:63:6f:1e:70:25:e9:c2:51:89:85"
}