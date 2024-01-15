resource "hcloud_firewall" "general" {
  name = "general"

  rule {
    description     = "Allow SSH access from everywhere"
    direction       = "in"
    port            = 22
    protocol        = "tcp"
    destination_ips = []
    source_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }
  rule {
    description = "Allow DNS requests via TCP"
    direction   = "out"
    port        = 53
    protocol    = "tcp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }
  rule {
    description = "Allow DNS requests from server via UDP"
    direction   = "out"
    port        = 53
    protocol    = "udp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }
  rule {
    description = "Allow HTTP requests from server via TCP"
    direction   = "out"
    port        = 80
    protocol    = "tcp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }
  rule {
    description = "Allow HTTP requests from server via UDP"
    direction   = "out"
    port        = 80
    protocol    = "udp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }
  rule {
    description = "Allow HTTPS requests from server via TCP"
    direction   = "out"
    port        = 443
    protocol    = "tcp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }
  rule {
    description = "Allow HTTPS requests from server via UDP"
    direction   = "out"
    port        = 443
    protocol    = "udp"
    destination_ips = [
      "0.0.0.0/0",
      "::/0",
    ]
  }
}