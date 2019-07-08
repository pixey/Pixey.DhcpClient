FROM alpine

RUN apk add --no-cache dhcp && \
    touch /dhcp.leases

VOLUME /etc/dhcp/dhcpd.conf

ENTRYPOINT [ "/usr/sbin/dhcpd", "-4", "-f", "-cf /etc/dhcp/dhcpd.conf", "-lf", "/dhcp.leases" ]