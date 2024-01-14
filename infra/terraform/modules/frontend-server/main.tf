resource "hcloud_firewall" "frontend" {
  name = var.name

  #HTTP
  rule {
    description = "HTTP requests from  server via TCP"
    direction   = "in"
    port        = 80
    protocol    = "tcp"
    source_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }

  rule {
    description = "HTTP requests from  server via UDP"
    direction   = "in"
    port        = 80
    protocol    = "udp"
    source_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }

  #DNS
  rule {
    description = "DNS request from the server"
    direction   = "out"
    port        = 53
    protocol    = "tcp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }

  rule {
    description = "DNS request from the server"
    direction   = "out"
    port        = 53
    protocol    = "udp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }

  #HTTP
  rule {
    description = "HTTP requests from the server"
    direction   = "out"
    port        = 80
    protocol    = "tcp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }

  rule {
    description = "HTTP requests from the server"
    direction   = "out"
    port        = 80
    protocol    = "udp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }

  #HTTPS
  rule {
    description = "HTTPS requests from the server"
    direction   = "out"
    port        = 443
    protocol    = "tcp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }
  rule {
    description = "HTTPS requests from the server"
    direction   = "out"
    port        = 443
    protocol    = "udp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }

  #SSH connection
  rule {
    description = "SSH connection to the server"
    direction   = "in"
    port        = 22
    protocol    = "udp"
    source_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }

  #HTTPS
  rule {
    description = "HTTPS requests to server from frontend server via TCP"
    direction   = "in"
    port        = 443
    protocol    = "tcp"
    source_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }

  rule {
    description = "HTTPS requests to server from frontend server via UDP"
    direction   = "in"
    port        = 443
    protocol    = "udp"
    source_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }
}

resource "hcloud_server" "frontend" {
  name        = var.name
  location    = "nbg1"
  server_type = "cx11"
  image       = "ubuntu-22.04"
  network {
    network_id = var.network_id
    ip = var.private_ip
  }
  firewall_ids = [
    var.base_firewall_id,
    hcloud_firewall.frontend.id,
  ]
  ssh_keys = var.ssh_keys
}