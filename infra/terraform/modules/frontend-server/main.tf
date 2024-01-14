resource "hcloud_firewall" "frontend" {
  name = var.name

  rule {
    description = "Allow HTTP requests to server via TCP"
    direction   = "in"
    port        = 80
    protocol    = "tcp"
    source_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }
  rule {
    description = "Allow HTTP requests to server via UDP"
    direction   = "in"
    port        = 80
    protocol    = "udp"
    source_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }
  rule {
    description = "Allow HTTPS requests to server via TCP"
    direction   = "in"
    port        = 443
    protocol    = "tcp"
    source_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }
  rule {
    description = "Allow HTTPS requests to server via UDP"
    direction   = "in"
    port        = 443
    protocol    = "udp"
    source_ips = [
      "0.0.0.0/0",
      "::/0",
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
    var.general_firewall_id,
    hcloud_firewall.frontend.id,
  ]
  ssh_keys = var.ssh_keys
}