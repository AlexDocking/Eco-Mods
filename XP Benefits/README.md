# XP Benefits

Reward players for maintaining a high xp rate with extra carry capacity and a bigger stomach!

*The rewards are an optional QoL bonus for participating in the economy. There's no penalty for eating cheap food if you still want to.*

Unless your server has changed the default settings, it works as follows:

### Players:

You can earn +6000 extra calorie capacity, +30kg higher weight limit for carrying items on your person and carry +100% blocks in the hands.
The higher your food xp and housing xp, the more benefit you get. Now you've a reason to still buy expensive once you've levelled your skills - Keep the economy going!
You'll need both food *and* housing xp; neglecting either xp type will really hurt the amount of reward you get.

Your skill rates are scaled by the "maximum" xp scores, minus the food base gain, to work out how much benefit you get.
The maximum food is set at 120 (times server skill rate) and the maximum housing is set at 200 (times server skill rate). They are only 'maximum' in the sense that if you achieve it you'll get all of the listed benefit amount. If you can get higher, you'll keep earning more reward.

...Skip over the examples if you're not interested in how the exact figure is calculated...

##### Example 1:

If you have 5% of the food xp and 80% of the housing xp, you'll get sqrt(0.05 * 0.8) = 20% of the benefits -> +1200 extra calorie space, +6kg weight limit and +50% hands stack limit.
Eating better food will pay dividends compared with improving your housing score when it's already way ahead of your food.

##### Example 2:

If you have 100% of the maximum food xp and 0 housing xp, you'll get no benefits. Build a house! Those furniture makers deserve sales too!


### Server Owners:

You can configure how the mod is set up through the config file. Instructions can be found in the template file.
In the config you can set how much of each benefit to give and change how the food and housing xp are combined to give the benefit amount.
Instead of the geometric mean (default), if you wanted to you could scale the rewards by the sum of the food and housing, or either the food or housing alone. 
You can also change what values are used as the maximum food and housing, and whether player's xp should be capped to those numbers.

If you are running your server on Windows then you can configure the mod through the server GUI window that appears when you launch the server, or instead copy the contents of "Configs/Mods/XP Benefits/XPBenefits.eco.template" to a new file "Configs/Mods/XP Benefits/XPBenefits.eco" and change the values in there.

## Dependencies:

- Ecompatible (can be downloaded from https://mod.io/g/eco/m/ecompatible)

## Installation:

Unzip and drop it in the server directory.

## Uninstallation:

Go to "Mods/UserCode" and delete the folder "XP Benefits" and the Ecopedia xml files for XP Benefits in "Ecopedia/Mods"

### Credits:

**Special thanks** go to ArmoredStone and TheDu for their ideas.


Do get in touch if you have any problems or queries.

Enjoy!


## Changelog

### v2.0.1

- Fix crash on startup if the mod can't find existing Ecopedia pages

### v2.0.0

- Update for Eco v0.11.0.0. Added new ecopedia pages and tooltips (English only). Replaced the Settings.cs file with a new config file that can be configured through the server GUI. 'Override' files have been removed as that is now handled through Ecompatible (now a dependency)

### v1.2.1

- Update for Eco v0.10.2.3

### v1.2.0

- Update for Eco v0.10.0. Remove option to exclude the bonus from shovels (IncreaseShovelSize) - now it is always applied

### v1.1.0

- Make sweeping hands pick up the extra rocks you should be allowed to hold
- Update uninstallation instructions

### v1.0.1

- Fix issue with stat modifiers potentially not being removed on log out (which would have appeared in the console log)

### v1.0.0

- Initial release