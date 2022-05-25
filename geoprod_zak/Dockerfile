# `python-base` sets up all our shared environment variables
FROM python:3.10

RUN apt-get update && apt-get install python3-psycopg2

RUN groupadd -g 1666 backend && useradd -ms /bin/bash -u 1667 -g backend backend 
RUN mkdir /app
RUN chown -R backend:backend /app
USER backend


# python
ENV PYTHONUNBUFFERED=1 \
    # prevents python creating .pyc files
    PYTHONDONTWRITEBYTECODE=1 \
    \
    # poetry
    # https://python-poetry.org/docs/configuration/#using-environment-variables
    POETRY_VERSION=1.1.13 \
    PIP_NO_CACHE_DIR=off \
    PIP_DISABLE_PIP_VERSION_CHECK=on \
    # do not ask any interactive question
    POETRY_NO_INTERACTION=1 \
    PATH="${PATH}:/home/backend/.local/bin"

# copy project requirement files here to ensure they will be cached.
WORKDIR /app
COPY poetry.lock pyproject.toml ./
RUN pip install poetry==${POETRY_VERSION}

RUN poetry config virtualenvs.create false
# install runtime deps - uses $POETRY_VIRTUALENVS_IN_PROJECT internally
RUN poetry install --no-dev

# `production` image used for runtime
COPY src/ /app/src
USER root
RUN chown -R backend:backend /app
USER backend
CMD ["uvicorn", "src.main:api", "--host", "0.0.0.0"]
