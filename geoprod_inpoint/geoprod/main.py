import uvicorn
from fastapi import FastAPI, Query

from geoprod.models import GeoInput

api = FastAPI()


@api.post('/oil')
async def oil(request: GeoInput, from_: str = Query(...), to_: str = Query(...)):
    return request, from_, to_


@api.post('/gas')
async def gas(request: GeoInput, from_: str = Query(...), to_: str = Query(...)):
    return request, from_, to_


if __name__ == '__main__':
    uvicorn.run('main:api', reload=True, debug=True)
