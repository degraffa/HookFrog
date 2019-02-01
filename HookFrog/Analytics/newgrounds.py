import numpy as np
import matplotlib as mpl
import matplotlib.pyplot as plt
import pandas as pd

mpl.style.use('ggplot')
fsize = 14
# ===============================================
# read data from csv file into DataFrames
# ===============================================

#data = pd.read_csv("newgrounds-analysis.csv")
data_time = pd.read_csv("active_time.csv")

players_remaining = np.multiply(100, data_time["percentage players"])
A_remaining = np.multiply(100, data_time["percent A"])
B_remaining = np.multiply(100, data_time["percent B"])
active_time = data_time["active time"]

# ===============================================
# setup plot params & show plot
# ===============================================

x = active_time
y = players_remaining
ya = A_remaining
yb = B_remaining

fig, ax = plt.subplots()

ax.set_xlim(0, 2200)
ax.set_ylim(0, 100)

ax.xaxis.set_major_locator(plt.MultipleLocator(300))
ax.xaxis.set_minor_locator(plt.MultipleLocator(60))
ax.tick_params(labelsize=fsize)

ax.set_xlabel("Active Time (s)", labelpad=15, fontsize=fsize)
ax.set_ylabel("% of Players Remaining", labelpad=15, fontsize=fsize)

# plt.title("Burndown by Time (All Players)", pad=15, fontsize=fsize)
plt.title("Burndown by Time (split by AB value)", pad=15, fontsize=fsize)

# plt.plot(x, y, lw=2)
plt.plot(x, ya, 'b-', lw=2, label="Condition A")
plt.plot(x, yb, 'r-', lw=2, label="Condition B")

plt.subplots_adjust(right=0.81, top=0.81)
plt.legend(fontsize=fsize)
plt.show()