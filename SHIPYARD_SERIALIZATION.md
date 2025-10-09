# Shipyard serialization and loaded ship behavior

When a ship is loaded from a YAML manifest (either via the Shipyard Console UI or the server-side load path), it follows the same docking and setup flow as a purchased shuttle, but with one important difference: loaded ships are not kept as members of the station.

Why: The purchase-from-file path briefly adds the grid to the console's station to initialize ownership and IFF. If this membership is left in place, station-wide events (e.g., alerts, station rules) can incorrectly target the loaded ship. To prevent that, the load flow removes station membership after docking.

Implications:
- Loaded ships behave like independent shuttles (similar to purchased shuttles) and won't be affected by station-wide game rules/events.
- Round persistence or other explicit flows may still assign station membership when desired; the YAML load flow will not.

If you add new ship load flows, ensure they do not permanently add the loaded grid to a station unless explicitly intended.

