from datetime import date
import uvicorn
from fastapi import FastAPI, Query
from fastapi.responses import PlainTextResponse
from requests import post

from src.models import GeoInput, Mode

api = FastAPI()


def get_mode_units(mode: Mode):
    return "m^3" if mode == Mode.CubeMeter else "l"


# type: ignore
def make_request(url: str, port: int, data: str, params: dict[str, str]):
    return post(f"http://{url}:{port}", data=data, params=params)


def make_plot_request(from_: str, mode: Mode, data: str):
    return post(f"http://plot:80/{from_}/{get_mode_units(mode)}", data=data, headers={'content-type': 'application/json'})


def handle_request(service_name: str, service_port: int, geo_input: str, from_: date, to_: date, plot: bool, mode: Mode = Mode.CubeMeter):
    req = make_request(service_name, service_port, geo_input,
                       {"from_": from_, "to_": to_, "mode": mode.value})

    if plot:
        return PlainTextResponse(make_plot_request(service_name, mode, req.text.encode('utf8')).text.encode('utf8'))
    return req.json()


@api.post('/oil')
async def oil(request_: GeoInput, mode: Mode = Query(...), from_: date = Query(...), to_: date = Query(...), plot: bool = False):
    return handle_request("oil", 80, request_.json(), from_, to_, plot, mode)


@api.post('/liq')
async def liq(request_: GeoInput, mode: Mode = Query(...), from_: date = Query(...), to_: date = Query(...), plot: bool = False):
    return handle_request('liq', 8000, request_.json(), from_, to_, plot, mode)


@api.post('/zak')
async def zak(request_: GeoInput, from_: date = Query(...), to_: date = Query(...), plot: bool = False):
    return handle_request('zak', 8000, request_.json(), from_, to_, plot)


@api.post('/com')
async def com(request_: GeoInput, from_: date = Query(...), to_: date = Query(...), plot: bool = False):
    return handle_request('com', 8000, request_.json(), from_, to_, plot)


if __name__ == '__main__':
    uvicorn.run('main:api', reload=True, debug=True)
