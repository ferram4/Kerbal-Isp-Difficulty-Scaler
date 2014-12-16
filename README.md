Kerbal Isp Difficulty Scaler, v1.4.2
Copyright 2013, Michael Ferrara, aka Ferram4


    This file is part of Kerbal Isp Difficulty Scaler.

    Kerbal Isp Difficulty Scaler is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Kerbal Isp Difficulty Scaler is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Kerbal Isp Difficulty Scaler.  If not, see <http://www.gnu.org/licenses/>.


This plugin allows a player to globally scale the specific impulses of all engines and rcs blocks to increase difficulty / augment realism.

Why?

It allows us to manipulate the difficulty of the game by requiring larger mass ratios to make the same manuevers; we can't change the delta-V requirements, but we can make a given rocket produce less delta-V, which has the same effect in the end.
With this plugin, it is possible to require realistic size rockets to get to orbit--a rocket that would be able to get to the Mun and back, for example, will only be able to reach orbit if the Real Life preset is selected.


Source can be found at https://github.com/ferram4/Kerbal-Isp-Difficulty-Scaler

*****************************************************
****** INSTALLING KERBAL ISP DIFFICULTY SCALER ******
*****************************************************

Merge the GameData folder with the existing one in your KSP directory.  KSP will then load it as an add-on.


********************************
****** EXCITING FEATURES! ******
********************************

* Define IspPresets, which allow you to make the following changes:
	--Vac Isp Multiplier		- How much the vacuum Isp of the engine is scaled; >1 makes engines more efficient than stock, <1 makes them less efficient than stock
	--Atm Isp Multiplier		- Same as above, but at 1 atm (Kerbin Sea Level)
	--Extend Curve to Zero Isp	- Choose whether to have Isp bottom out at 1 atm or continue decreasing until it hits 0 (does nothing if Isp increases with atmospheric pressure)

	--Thrust Corrector		- Choose whether to have thrust vary with Isp while fuel flow remains constant (as it should) or keep the stock constant thrust with variable fuel flow; does not affect air-breathing engines
	--Thrust Corrector Options	- Can have engines rated for vacuum (creates rated thrust in vacuum and less on the pad) or rated for atmosphere (creates rated thrust on pad and more in vacuum) based on Isp and/or thrust

		**Isp Cutoff		- Isps above this will be considered "vac-rated" while Isps below this will be considered "atm-rated"
		**Thrust Cutoff		- Rated thrusts below this will be considered "vac-rated" while thrusts above this will be considered "atm-rated"

		^Note: If both Isp and thrust cutoffs are set the engine must have an Isp below the Isp Cutoff AND a thrust above the Thrust Cutoff to be "atm-rated"; this means only low-efficiency, high-thrust engines will be "atm-rated" (based on your definitions of "low" efficiency and "high" thrust)
	 
	
* Select a different Preset for each saved game!

* Preset selection / modification done in Space Center view, to help remove the temptation to change scaling to help the rocket that just can't quite make it

* Compatibility with Kerbal Engineer and MechJeb; it still takes 4.3 km/s dV to get to Kerbin orbit in the stock game, and it still takes 3.5 km/s dV to get to Kerbin orbit with FAR installed

* Default Presets, including conversions that require real-life mass ratios and conversions to handle FAR's lower drag

Known Issues:
Isps do not update in the VAB / SPH Parts List; however, Kerbal Engineer and MechJeb will still provide correct dV and Isp readouts

Conflicts heavily with Arcturus Thrust Corrector

***********************
****** CHANGELOG ******
***********************
v1.4.2
Compatibility with KSP 0.90

v1.4.1
Compatibility with KSP 0.25  
Update CompatibilityChecker  
Lock down on CompatibilityChecker warnings

v1.4
Some changes to make it play better with RF

v1.3.4.3
Compatibility with KSP 0.24.2

v1.3.4.2
Compatibility with KSP 0.24.1

v1.3.4.1
Bugfixes:
Included JsonFx.dll, which is required by ModStats
Relabeled ModStatistics.dll to allow simple overwriting for ModStats updates
Fixed buttons being added to toolbar after each flight

v1.3.4
Compatibility with KSP 0.24
Switched from dependency on Blizzy's toolbar to using stock toolbar instead

v1.3.3
Compatibility with KSP 0.23.5

V1.3.2
Fixed a serious issue with the Extend To Zero Isp option that would make launches impossible

V1.3.1
Fixed an issue where not setting any Isp or Thrust cutoffs would cause all engines to be atm rated, rather than vac rated as intended

V1.3
Updated to KSP 0.23
Optimized method of handling complex Isp curves
First attempt at support for Modular Fuels engines and HydraEngineController (used by B9 SABREs)
GUI upgraded to use blizzy78's excellent Toolbar plugin


v1.2
Default preset thrust corrector thrust cutoff options modified to rate small SRB, LV-T30 and LV-T45 in atm
More complicated Isp curves are now preserved
Jet engines are not affected by thrust corrector

v1.1
Added Thrust Corrector
Default Presets updated to include thrust corrector and cutoff options
Reflectively detects Arcturus Thrust Corrector and throws a warning window up if found

v1.0
Initial release for 0.21.1+