import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs

Menu {
    title: "Presets"

    Action {
        text: "Save Preset"
        onTriggered: {
            presetAddDialog.open();
        }
    }

    Menu {
        title: "Load..."
        id: presetsListMenu
        enabled: presetsMenuInstantiator.count > 0

        Instantiator {
            id: presetsMenuInstantiator
            model: program.presets
            delegate: MenuItem {
                text: modelData.name
                onTriggered: function() {
                    program.presetLoad(modelData);
                    program.ruleListUpdated();
                    program.filtersUpdated();
                }
            }

            onObjectAdded: (index, object) => presetsListMenu.insertItem(index, object);
            onObjectRemoved: (index, object) => presetsListMenu.removeItem(object)
        }
    }
}
