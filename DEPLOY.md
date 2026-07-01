# Deploying HobbyApp Backend (Debian 13 VPS + Docker + Caddy)

This deploys the API + PostgreSQL + a Caddy reverse proxy that auto-provisions a
free HTTPS certificate for **hobbyapp.tech**.

```
phone ── https://hobbyapp.tech ──▶ Caddy (TLS, :443) ──▶ API (:8080) ──▶ Postgres
                                       (only Caddy is exposed to the internet)
```

## 0. Prerequisites

- A VPS (Hostinger KVM1 is fine) running Debian 13, with root/sudo SSH access.
- The domain **hobbyapp.tech** with a DNS **A record** pointing at the VPS's
  public IP. (In your domain registrar's DNS panel: `A  @  <VPS_PUBLIC_IP>`.)
  Verify it propagated: `dig +short hobbyapp.tech` should print your VPS IP.

## 1. Point DNS at the server

Do this first — Let's Encrypt validates the domain over the public internet, so
the A record must resolve to the VPS before you start Caddy.

## 2. Install Docker on the VPS

SSH in, then:

```bash
sudo apt update && sudo apt upgrade -y
sudo apt install -y ca-certificates curl git
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/debian/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/debian $(. /etc/os-release && echo $VERSION_CODENAME) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo systemctl enable --now docker
docker --version && docker compose version
```

## 3. Firewall

```bash
sudo apt install -y ufw
sudo ufw allow OpenSSH      # keep SSH (port 22) open
sudo ufw allow 80/tcp       # Caddy needs 80 for the certificate challenge
sudo ufw allow 443/tcp      # HTTPS
sudo ufw enable
sudo ufw status
```
Postgres (5432) stays closed — it's only reachable inside the Docker network.

## 4. Get the code

```bash
git clone <your-backend-repo-url> hobby-app-backend
cd hobby-app-backend
```
(Or `scp` the folder up if the repo isn't pushed anywhere.)

## 5. Create the production secrets

```bash
cp .env.example .env
nano .env
```
Set strong values:
- `POSTGRES_PASSWORD` — a strong random password.
- `JWT_KEY` — generate one: `openssl rand -hex 64`.

`.env` is gitignored — keep it only on the server.

## 6. Confirm the domain in the Caddyfile

`Caddyfile` already targets `hobbyapp.tech`. If you ever change domains, edit it
there. (Optional: add an email for ACME notices by putting `email you@example.com`
inside a global options block at the top of the Caddyfile.)

## 7. Launch

```bash
sudo docker compose -f docker-compose.prod.yml up -d --build
```

This builds the API image, starts Postgres, runs the API (which **auto-applies
EF Core migrations on startup** — no manual `dotnet ef` needed), and starts Caddy,
which fetches the TLS certificate within a few seconds.

Watch it come up:
```bash
sudo docker compose -f docker-compose.prod.yml logs -f
```
Look for the API listening on `:8080` and Caddy reporting `certificate obtained`.

## 8. Verify

```bash
curl -i https://hobbyapp.tech/api/auth/me      # expect 401 (no token) — proves TLS + routing work
```
A `401 Unauthorized` over **https** means everything is wired correctly.

## 9. Point the mobile app at production

In the `hobby-app` project set:
```
EXPO_PUBLIC_API_URL=https://hobbyapp.tech
```
and rebuild the app.

---

## Operations cheatsheet

| Task | Command |
|---|---|
| View logs | `sudo docker compose -f docker-compose.prod.yml logs -f` |
| Restart everything | `sudo docker compose -f docker-compose.prod.yml restart` |
| Stop | `sudo docker compose -f docker-compose.prod.yml down` |
| Deploy new code | `git pull && sudo docker compose -f docker-compose.prod.yml up -d --build` |
| Backup database | `sudo docker exec hobbyapp-postgres pg_dump -U $POSTGRES_USER hobbyapp > backup_$(date +%F).sql` |
| Restore database | `cat backup.sql \| sudo docker exec -i hobbyapp-postgres psql -U $POSTGRES_USER -d hobbyapp` |

## Notes

- **Migrations** run automatically when the API container starts. To add a new
  migration during development, use the `dotnet ef migrations add` command from
  the project README, commit it, then `git pull` + rebuild on the server.
- **CORS** is not configured (the native mobile app doesn't need it). If you host
  the Expo **web** build on another origin, add a CORS policy in `Program.cs`.
- **Data** lives in the `hobbyapp-pgdata` Docker volume and survives restarts and
  rebuilds. `docker compose down -v` would delete it — don't use `-v` in prod.
