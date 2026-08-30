#!/bin/sh

CERTS_DIR="./certs"
CERT_FILE="$CERTS_DIR/fullchain.pem"
KEY_FILE="$CERTS_DIR/privkey.pem"

if [ ! -d "$CERTS_DIR" ]; then
    echo '===> Creating certs directory...'
    mkdir -p "$CERTS_DIR"
fi

if [ ! -f "$CERT_FILE" ] || [ ! -f "$KEY_FILE" ]; then
    echo '===> SSL certificates missing. Generating self-signed certificates...'

    apk add --no-cache openssl && \

    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
        -keyout "$KEY_FILE" \
        -out "$CERT_FILE" \
        -subj '/CN=localhost'
else
    echo '===> Using existing SSL certificates.'
fi