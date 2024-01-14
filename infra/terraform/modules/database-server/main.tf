resource "hcloud_firewall" "database" {
  name = var.name

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
    description = "HTTPS requests from the  server via TCP"
    direction   = "out"
    port        = 443
    protocol    = "tcp"
    source_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }

  rule {
    description = "HTTPS requests from the  server via UDP"
    direction   = "out"
    port        = 443
    protocol    = "udp"
    source_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }

  #Backend
  rule {
    description = "Allow requests to server from backend server via TCP"
    direction   = "in"
    port        = 5432
    protocol    = "tcp"
    source_ips = [
      "${var.backend_ip}/32",
    ]
  }
}

resource "hcloud_server" "database" {
  name        = var.name
  location    = "nbg1"
  server_type = "cx11"
  image       = "ubuntu-22.04"
  network {
    network_id = var.network_id
    ip         = var.private_ip
  }
  firewall_ids = [
    var.base_firewall_id,
    hcloud_firewall.database.id,
  ]
}
