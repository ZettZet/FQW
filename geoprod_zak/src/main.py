from datetime import date, datetime
from typing import Optional
from fastapi import FastAPI, Query
from sqlmodel import Session, select, between
import uvicorn

from src.models import GeoInput, ZakWell, ZakWellCalculated, ZakWellValue
from sqlalchemy.future import Engine

api = FastAPI()

db: Optional[Engine]


@api.on_event("startup")
async def on_start():
    import os
    from sqlmodel import create_engine

    name = os.getenv('POSTGRES_USER')
    password = os.getenv('POSTGRES_PASSWORD')
    host = os.getenv('POSTGRES_HOST')
    port = os.getenv('POSTGRES_PORT')
    db_name = os.getenv('POSTGRES_DB')

    connection_string = f'postgresql://{name}:{password}@{host}:{port}/{db_name}'
    global db
    db = create_engine(connection_string)


def wells_satisfied(from_: date, to_: date, id_: str, name: str) -> list[ZakWellValue]:
    statement = select(ZakWell) \
        .where(ZakWell.well == id_) \
        .where(ZakWell.well_num == name) \
        .where(between(ZakWell.dt, from_, to_))

    result: list[ZakWellValue] = []
    with Session(db) as session:
        for well in session.exec(statement):
            result.append(ZakWellValue(
                well=well.well, well_num=well.well_num, zak=well.zak, dt=well.dt))
    return result


def logic(from_: date, to_: date, regions: GeoInput):
    wells_with_result: list[ZakWellCalculated] = []

    for region in regions.regions:
        for well in region.wells:

            satisfied: list[ZakWellValue] = wells_satisfied(
                from_, to_, well.id, well.name)

            result = 0.0
            result_unweighted = 0.0

            for ws in satisfied:
                weight = 1.0 if len(well.weights) == 0 else min(well.weights.items(), key=lambda x: datetime.strptime(
                    x[0], '%Y-%m-%d') - datetime.now())[1]

                result += ws.zak * weight
                result_unweighted += ws.zak
            wells_with_result.append(ZakWellCalculated(
                region=region.id, id=well.id, name=well.name, result=result, result_uw=result_unweighted))
    return wells_with_result


@api.post('/')
async def handler(request_: GeoInput, from_: date = Query(...), to_: date = Query(...)):
    return logic(from_, to_, request_)


if __name__ == '__main__':
    uvicorn.run('main:api', reload=True, debug=True)
