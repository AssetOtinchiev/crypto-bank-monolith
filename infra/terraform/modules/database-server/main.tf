resource "hcloud_firewall" "database" {
  name = var.name

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
    var.general_firewall_id,
    hcloud_firewall.database.id,
  ]
}