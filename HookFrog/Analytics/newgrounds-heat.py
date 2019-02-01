import numpy as np
import matplotlib as mpl
import matplotlib.pyplot as plt
import pandas as pd
import json

fsize = 16

# Reading the json as a dict
with open('full-data.json') as json_data:
    data = json.load(json_data)

player_actions = pd.DataFrame(data['player_actions'])
user_abvalues = pd.DataFrame(data['abtesting'])
user_A = user_abvalues[user_abvalues["ab_testing_value"] == 1]
user_B = user_abvalues[user_abvalues["ab_testing_value"] == 2]

level107_deaths = player_actions[(player_actions["quest_id"] == 107) & (player_actions["action_id"] == 0)]
level107_deathsA = level107_deaths[~level107_deaths["user_id"].isin(user_A["user_id"])]
level107_deathsB = level107_deaths[~level107_deaths["user_id"].isin(user_B["user_id"])]

level107_death_locations = level107_deaths["action detail"].apply(lambda ad_json: pd.read_json(ad_json)["location"])

level107_death_locationsA = level107_deathsA["action detail"].apply(lambda ad_json: pd.read_json(ad_json)["location"])
level107_death_locationsB = level107_deathsB["action detail"].apply(lambda ad_json: pd.read_json(ad_json)["location"])
# ===============================================
# get (x,y) from flattened json
# ===============================================
level107_death_locations_x = [ x for x in level107_death_locations["x"]]
level107_death_locations_y = [ y for y in level107_death_locations["y"]]

level107_death_locations_xA = [ x for x in level107_death_locationsA["x"]]
level107_death_locations_yA = [ y for y in level107_death_locationsA["y"]]

level107_death_locations_xB = [ x for x in level107_death_locationsB["x"]]
level107_death_locations_yB = [ y for y in level107_death_locationsB["y"]]
# ===============================================
# setup plot params & show plot
# ===============================================
fig, ax = plt.subplots()
# ax.scatter(death_location_level1_x, death_location_level1_y, alpha=0.5)

x = level107_death_locations_x
y = level107_death_locations_y

xA = level107_death_locations_xA
yA = level107_death_locations_yA

xB = level107_death_locations_xB
yB = level107_death_locations_yB

plt.title("Heatmap of Player Deaths in Forest Level 1 (All Players)", pad=15, fontsize=fsize)
plt.hist2d(x, y, bins=(25, 10), cmap=plt.cm.Reds, range=[[-25, 225],[-50, 50]])
cbar = plt.colorbar()
cbar.ax.tick_params(labelsize=fsize)

plt.axis('off')

# ax.set_xlim(-25, 225)
# ax.set_ylim(-50, 50)

# plt.xlim([-25, 225])
# plt.ylim([-50, 50])

plt.subplots_adjust(top=0.58)
plt.gca().set_aspect('equal', adjustable='box')
plt.show()