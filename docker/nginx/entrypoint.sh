#!/bin/bash
set -e

echo "Starting nginx with NGINX_HOST=${NGINX_HOST}"

if [ -n "${NGINX_HOST}" ]; then
    echo "Substituting NGINX_HOST in nginx.conf..."
    sed -i "s/\${NGINX_HOST}/${NGINX_HOST}/g" /etc/nginx/nginx.conf
else
    echo "NGINX_HOST not set, using localhost"
    sed -i "s/\${NGINX_HOST}/localhost/g" /etc/nginx/nginx.conf
fi

echo "Final nginx.conf server_names:"
grep -A0 "server_name" /etc/nginx/nginx.conf

exec docker-entrypoint.sh nginx -g 'daemon off;'