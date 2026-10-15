import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs

Menu {
    title: "&Rules"

    Action {
        text: "Add Rule"
        onTriggered: {
            ruleAddDialog.open()
        }
    }
    Action {
        text: "Clear Rule List"
        onTriggered: {
            program.rules.clear();
            program.ruleListUpdated();
        }
    }
}
