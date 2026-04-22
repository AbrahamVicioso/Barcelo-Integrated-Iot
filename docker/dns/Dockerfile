FROM alpine:3.19

RUN apk add --no-cache dnsmasq

COPY dnsmasq.conf /etc/dnsmasq.conf

EXPOSE 53/udp

CMD ["dnsmasq", "-k", "--no-daemon", "--log-queries"]