import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs

Dialog {
    id: presetAddDialog

    title: "Add Preset"
    modal: true
    focus: true
    anchors.centerIn: parent
    closePolicy: Popup.CloseOnEscape
    standardButtons: Dialog.Ok | Dialog.Cancel

    onAccepted: function () {
        program.presetAdd(presetAddName.text, (presetIncludeFilters.checkState === Qt.Checked));
    }

    GridLayout {
        columns: 2
        rowSpacing: 5
        columnSpacing: 5

        Label {
            text: "Preset Name:"
        }
        TextField {
            id: presetAddName
            Layout.fillWidth: true
        }
        Label {
            text: ""
        }
        CheckBox {
            id: presetIncludeFilters
            text: "Include filter settings"
        }
    }
}