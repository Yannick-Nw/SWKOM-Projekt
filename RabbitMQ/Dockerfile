FROM rabbitmq:3-management

RUN mkdir -p /etc/rabbitmq/conf.d
RUN chown -R rabbitmq:rabbitmq /etc/rabbitmq/conf.d
COPY logging.conf /etc/rabbitmq/conf.d
