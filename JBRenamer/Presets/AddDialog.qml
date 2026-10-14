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
    width: 500

    onOpened: function () {
        presetAddName.text = "";
        presetIncludeFilters.checkState = Qt.Unchecked;
    }
    
    footer: DialogButtonBox {
        Button {
            text: "Save"
            DialogButtonBox.buttonRole: DialogButtonBox.ApplyRole
            onClicked: function() {
                if (presetAddName.validationError !== "") {
                    return;
                }
                program.presetAdd(presetAddName.text, (presetIncludeFilters.checkState === Qt.Checked));

                presetAddDialog.close();
            }
        }
        Button {
            text: "Cancel"
            DialogButtonBox.buttonRole: DialogButtonBox.RejectRole
        }
    }

    GridLayout {
        columns: 2
        rowSpacing: 5
        columnSpacing: 5
        anchors.fill: parent

        Label {
            Layout.alignment: Qt.AlignTop
            Layout.leftMargin: 10
            text: "Preset Name:"
        }
        ColumnLayout {
            Layout.alignment: Qt.AlignTop
            Layout.fillWidth: true

            TextField {
                property string validationError: ""
                id: presetAddName
                Layout.fillWidth: true
                onTextChanged: function () {
                    validationError = program.presetValidateName(text)
                }
            }
            Label {
                id: presetAddNameError
                Layout.fillWidth: true
                width: parent.width * 0.9
                Layout.preferredWidth: width
                wrapMode: Text.Wrap
                text: presetAddName.validationError
                color: (Application.styleHints.colorScheme === Qt.ColorScheme.Light ? "red" : "tomato");
            }
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