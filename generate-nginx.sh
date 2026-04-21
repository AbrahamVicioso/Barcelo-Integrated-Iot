#!/bin/bash
set -e

NGINX_HOST=${NGINX_HOST:-localhost}

cat > ./docker/nginx/nginx-generated.conf << 'NGINX_EOF'
events {
    worker_connections 1024;
}

http {
    include       /etc/nginx/mime.types;
    default_type  application/octet-stream;

    sendfile        on;
    keepalive_timeout 65;

    upstream thingsboard {
        server thingsboard-ce:8080;
    }

    upstream api_gateway {
        server api-gateway:5019;
    }

    upstream ntfy_backend {
        server ntfy:80;
    }

    # ThingsBoard
    server {
        listen 80;
        server_name __THINGBOARD_HOST__;

        location / {
            proxy_pass http://thingsboard;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }
    }

    # API Gateway
    server {
        listen 80;
        server_name __API_HOST__;

        location / {
            proxy_pass http://api_gateway;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }
    }

    # Ntfy
    server {
        listen 80;
        server_name __NTFY_HOST__;

        location / {
            proxy_pass http://ntfy_backend;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }
    }
}
NGINX_EOF

# Substitute placeholders
sed -i "s/__THINGBOARD_HOST__/thingsboard.${NGINX_HOST}/g" ./docker/nginx/nginx-generated.conf
sed -i "s/__API_HOST__/api.${NGINX_HOST}/g" ./docker/nginx/nginx-generated.conf
sed -i "s/__NTFY_HOST__/ntfy.${NGINX_HOST}/g" ./docker/nginx/nginx-generated.conf

echo "Generated nginx config with NGINX_HOST=${NGINX_HOST}"
cat ./docker/nginx/nginx-generated.conf | grep server_name