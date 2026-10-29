#!/bin/sh
set -e

# Default BACKEND_URL if not set
BACKEND_URL=${BACKEND_URL:-}

# nginx proxy_pass needs a scheme; Render's hostport value has none
case "$BACKEND_URL" in
  http://*|https://*|"") ;;
  *) BACKEND_URL="http://${BACKEND_URL}" ;;
esac

echo "==> BACKEND_URL=${BACKEND_URL}"

# The browser calls /api/v1 on this origin; nginx proxies it to the backend,
# so the internal backend address is never sent to the browser.
# Substitute BACKEND_URL in nginx config
envsubst '$BACKEND_URL' < /etc/nginx/conf.d/default.conf.template > /etc/nginx/conf.d/default.conf

echo "==> Generated nginx config:"
cat /etc/nginx/conf.d/default.conf

exec nginx -g 'daemon off;'
