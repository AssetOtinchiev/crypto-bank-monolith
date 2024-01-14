resource "hcloud_firewall" "backend" {
  name = var.name

  rule {
    description = "Allow HTTP requests to server from frontend server via TCP"
    direction   = "in"
    port        = 80
    protocol    = "tcp"
    source_ips = [
      "${var.frontend_ip}/32",
    ]
  }
  rule {
    description = "Allow HTTP requests to server from frontend server via UDP"
    direction   = "in"
    port        = 80
    protocol    = "udp"
    source_ips = [
      "${var.frontend_ip}/32",
    ]
  }
  rule {
    description = "Allow HTTPS requests to server from frontend server via TCP"
    direction   = "in"
    port        = 443
    protocol    = "tcp"
    source_ips = [
      "${var.frontend_ip}/32",
    ]
  }
  rule {
    description = "Allow HTTPS requests to server from frontend server via UDP"
    direction   = "in"
    port        = 443
    protocol    = "udp"
    source_ips = [
      "${var.frontend_ip}/32",
    ]
  }
  rule {
    description = "Allow connect to database server"
    direction   = "out"
    port        = 5432
    protocol    = "tcp"
    destination_ips = [
      "${var.database_ip}/32",
    ]
  }
}

resource "hcloud_server" "backend" {
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
    hcloud_firewall.backend.id,
  ]
  ssh_keys = var.ssh_keys
}