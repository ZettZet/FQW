import { moment, serialize, serve } from "./deps.ts";
import {
  GeoInput,
  GeoQueryString,
  LiqWell,
  LiqWellCalculated,
  LiqWellValue,
  Mode,
  Region,
} from "./models.ts";
import { sql } from "./db_connection.ts";

const trim = (from: string, value: string) => {
  let result = from;
  if (result.startsWith(value)) {
    result = result.slice(value.length);
  }
  if (result.endsWith(value)) {
    result = result.slice(0, result.length - value.length);
  }

  return result;
};

const wellSatisfied = async (
  from_: Date,
  to_: Date,
  id: string,
  name: string,
  mode: Mode,
) => {
  name = trim(name, '"');
  id = trim(id, '"');

  const qs = await sql<LiqWell[]>
    `select * from "LiqWell" where well=${id} and well_num=${name} and dt between ${
      moment.moment(from_).format("YYYY-DD-MM")
    } and ${moment.moment(to_).format("YYYY-DD-MM")}`;

  const res: LiqWellValue[] = qs.map((item) => {
    return {
      id: item.well,
      name: item.well_num,
      dt: item.dt,
      liq: (mode == Mode.CubeMeter ? item.liq : item.liqton),
    };
  });
  return res;
};

const logic = async (query: GeoQueryString, regions: Region[]) => {
  const wellsWithResult: LiqWellCalculated[] = [];

  for (const region of regions) {
    for (const well of region.wells) {
      if (well.id == null || well.name == null) {
        continue;
      }

      const satisfied = await wellSatisfied(
        query.from_,
        query.to_,
        well.id,
        well.name,
        query.mode,
      );

      let result = 0.0;
      let result_unweighted = 0.0;

      for (const ws of satisfied) {
        const weights = new Map(
          Array.from(well.weights, ([k, v]: [string, number]) => {
            return [moment.moment().diff(k), v];
          }),
        );

        const minKey = Math.min(...Array.from(weights.keys())); // (...((...(ANГЕЛ_IZ_RАЯ)...))...)
        const weight = weights.get(minKey) || 1.0;

        result += weight * ws.liq;
        result_unweighted += ws.liq;
      }

      wellsWithResult.push({
        region: region.id,
        id: well.id,
        name: well.name,
        result: result,
        result_uw: result_unweighted,
      });
    }
  }
  return wellsWithResult;
};

const handler = async (req: Request): Promise<Response> => {
  const params = new URL(req.url).searchParams;
  const from_str = params.get("from_");
  const to_str = params.get("to_");
  const mode_str = params.get("mode");

  if (from_str == null || to_str == null || mode_str == null) {
    throw new Error("null value appears");
  }

  const from_ = new Date(from_str);
  const to_ = new Date(to_str);
  const mode = Number(mode_str) as Mode;

  const qs: GeoQueryString = { from_, to_, mode };
  const gi: GeoInput = new GeoInput().fromJSON(await req.json());
  // const regions = Array.from(
  //   (await req.json()).regions,
  //   (str: string) => new Region().fromJSON(str),
  // );

  return new Response(JSON.stringify(await logic(qs, gi.regions)));
};

serve.serve(handler);
