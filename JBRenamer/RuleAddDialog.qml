import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs
// The ../ is required here because thats how the files end up laid out in the bin/Debug/net10.0/ directory
import "../Rules" as Rules 

Dialog {
    id: ruleAddDialog
    title: "Add Rule"
    modal: true
    focus: true
    anchors.centerIn: parent
    closePolicy: Popup.CloseOnEscape
    standardButtons: Dialog.Ok | Dialog.Cancel
    
    onAccepted: function() {
        ruleConfig.children[ruleConfig.currentIndex].saveRule();
        program.ruleListUpdated();
    }
    
    width: 640
    height: 480
    
    RowLayout {
        anchors.fill: parent
        
        ListView {
            id: ruleTypeSelect
            Layout.fillHeight: true
            Layout.preferredWidth: 150
            orientation: Qt.Vertical
            delegate: ItemDelegate {
                width: ruleTypeSelect.width
                height: ruleTypeSelectItemText.height
                Text {
                    id: ruleTypeSelectItemText
                    font.bold: index === ruleConfig.currentIndex
                    color: palette.text
                    padding: 5

                    text: name
                }
                onClicked: ruleConfig.currentIndex = index
            }
            highlight: Rectangle {
                width: 180; height: 40
                color: palette.accent; radius: 5
                y: ruleTypeSelect.currentItem.y
                Behavior on y {
                    SpringAnimation {
                        spring: 3
                        damping: 0.2
                    }
                }
            }
            focus: true
            
            model: ListModel {
                ListElement {
                    name: "Replace"
                    index: 1
                }
                ListElement {
                    name: "Regular Expression"
                    index: 2
                }
                ListElement {
                    name: "Prefix"
                    index: 3
                }
            }
        }
        
        StackLayout {
            id: ruleConfig
            currentIndex: 0

            Rules.RuleConfig {
                Label {
                    text: "Select a rule type"
                }
            }
            Rules.Replace {}
            Rules.RegExp {}
            Rules.Prefix {}
        }
    }
}
