FROM ubuntu:23.10

RUN apt update \
    && apt install -y wget \
    && apt install -y unzip \
    && apt install -y vim \
    && apt install -y openssh-client

RUN apt-get install --yes software-properties-common && \
    add-apt-repository --yes --update ppa:ansible/ansible && \
    apt-get install --yes ansible

ENV ANSIBLE_HOST_KEY_CHECKING=False

WORKDIR /src

ENTRYPOINT ["bash"]