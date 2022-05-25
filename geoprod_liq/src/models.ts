import { serialize } from "./deps.ts";

export class Well extends serialize.Serializable {
  @serialize.SerializeProperty()
  public id = ""; // well

  @serialize.SerializeProperty()
  public name = ""; //well_num

  @serialize.SerializeProperty({
    fromJSONStrategy: (weights_l) => {
      let m = new Map();
      for (let k of Object.keys(weights_l)) {
        m.set(k, weights_l[k]);
      }
      return m;
    },
  })
  public weights: Map<string, number> = new Map();
}

export class Region extends serialize.Serializable {
  @serialize.SerializeProperty()
  public id = "";

  @serialize.SerializeProperty({
    fromJSONStrategy: (wells_l) =>
      Array.from(
        wells_l,
        (well: any) => new Well().fromJSON(well),
      ),
  })
  public wells: Well[] = [];
}

export class GeoInput extends serialize.Serializable {
  @serialize.SerializeProperty({
    fromJSONStrategy: (regions_l) =>
      Array.from(regions_l, (region: any) => new Region().fromJSON(region)),
  })
  public regions: Region[] = [];
}

type LiqWell = {
  id: number;
  well: string;
  well_num: string;
  dt: Date;
  liq: number;
  liq_cum: number;
  liqton: number;
  liqton_cum: number;
};

type LiqWellValue = {
  id: string;
  name: string;
  dt: Date;
  liq: number;
};

type LiqWellCalculated = {
  region: string;
  id: string;
  name: string;
  result: number;
  result_uw: number;
};

export enum Mode {
  CubeMeter = 0,
  Tonne = 1,
}

type GeoQueryString = { from_: Date; to_: Date; mode: Mode };

export type { GeoQueryString, LiqWell, LiqWellCalculated, LiqWellValue };
