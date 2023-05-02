### Harry Bosch Presents
# Quick Actions

### Installation

1. Open the Package Manager in Unity.
2. Add Package via Git Url.
3. Paste `https://github.com/HarryBosch03/QuickActions.git` into the field.

### Usage

This is a lil package I made for personal use that lets you setup a 
small menu to quickly access files / folders / applications / websites 
within a Unity project. 

The menu can be found within Tools/Quick Actions

### This is what we are working with.

![img.png](https://lh3.googleusercontent.com/fife/APg5EObLvo6zecYE1NTYwlzz8Ozvyp6jFwKa3t0IPYxs3nMPljb-wc3CT44Sj3pkjGIF3ehyiyIj6DcTeyTRyovGyxBuyFu73dPpqZ77RnjHY2bbdSNV2FBhVvnq9wJ3-UDMuoYQwjAEjaswxvmcVmnm7j2YMclOIC-Vqiwwymggt6O9CBX1X_wckekQJFy-TrIrsvFciF6OgyvHImX3C2zXlh6M_gTgOQYF7TjO1Op0Y6_nbSmltG0Ntqkrww-HrutiaKVopYdWDTTZ8Yk0fDgFQqkFyP0JQOTfsXlzRWDpzLfD5KinuavRT3LY1PoV15EkVJxa2wc9aNHi_PQOGrgKCLtcPe8dV0TovUHcwlB3JEhYHo08vXqd8uW5jMA6IrWxcIzi9xe_yoRRF3NfrsQry_-2HsMFRZ2FIq7wXERWvUPjggOK33lEuzEcXiBbiHnPD6avUIpBwb_IUBkhq8FIsJTL_4Iut94_pykCmVUhyodTyA6a6C5gL9j-TCG8LrHcJaUzqs8gRVNmQfB1EU6I3Kc5Suk98CHQDkfiuYGoYKhN1GkjWcKccvKbd77V52bNyFmRDapdmwTlx16HixAezRza6FiScLssoirytA-j5kTN-RCHgDEWflQXaA0Tyz1rF0SvJrFVONy3wJqrKggiVgijFNVDLVCIXs3-tOT7g7i26WkrVDPM8WV9Gc4uQ-bSrXxQaWUHt90qRn_cORL2xqb3MxVD7GwSK9zHP3mYQoTTuut3iY1FC8uXDa_HhNGXo-Gf6k18uxuSieVMw8v63UO2LU5LQjXJxUd-Cv1R2iIQRuQoyfAbOIynLR8ii_Bvwy6b6qmHO_jC7pBX0a8LFK4WzzOXxFuSYEXqAC6C4X97ZY6v_CwdZrLcJ0eqfcB99B3_p8c1v55Ny63D9MDUasJNURQ1q2JKPTwohKv755QdOmDmlYgAp-XPhQyMEaHdwOIJvSvcpCfklKRQ6efyCjhV3QzNqAajHByiH0qJc7S5WKZVN1E-80Uhy0xHmdX8WT-e2mEG2BwrFxz7Xfo-DUnRyt74jeAQbO9KzQyv3FU_HilNy2AhCLAuHDB9iSWHfRxo3tj_vpEXtE5DCJ4OfZh7JZjk8Mq2HAiPePGJKg8fUWsthddwZ9q65SuwpLI_jtlUD-yiIqxknNTMXT9pisqcvsgbx9WCVDeVe6RPyAwD5f36ec4bwSV3E9To2l2fJULERMNnqnRyAxd66xBfW7InxwGpjpZwFgKyysY1oDSCIVc2-zzpn_qgaWcLCvxyO7H4RXn8CV7ujXPBVdj_eFbZbPgFh8Cna_UfG7lWyGLF3Zba1eRHviNCVM6vbSXJ1KKQzUiAFjh4VyYV02gEtoPjBCjGHArTA9JbNxkfrs-6a6h-MN8zh1205VfF_OTXE1oEA37OzCJGTVnU-5BriHaZWiJtj6P_QRh50HqQQC_QfxVceg-UmLEvwXRLdAmA-w1o_ILHgseG51MZ25wx1ZJojDxoh75KmCKOHOASNbwxwchb0QN6AT2PTSNHTCg=w1920-h969)

The First Field is where you can set the data asset used to draw the menu, 
this can be swapped out to have multiple different groupings of shortcuts.
The package comes with one by default, which if not there can be quickly grabbed
with the Quick Find Button that will show up.

Unity Objects can be added to the "Quick Open Assets" list, which will be
grouped by type.

### Links can be added in the format of:
- Directories relative to the user folder, or the Unity Projects "Assets" folder
- Files relative to the user folder, or the Unity Projects "Assets" folder
- Names of applications [Installed with a shortcut in 
"%appdata%\Microsoft\Windows\Start Menu\Programs" or 
"C:\ProgramData\Microsoft\Windows\Start Menu\Programs"]

If the item does not fit one of these, it will link to a website.

The "Add File..." and the "Add Directory..." will open a window to select a file 
or directory respectively to ass to the external links list.