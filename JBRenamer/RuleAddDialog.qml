import QtQuick
import QtQuick.Controls
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
                    font.bold: index == ruleConfig.currentIndex
                    padding: 5

                    text: name + " @ " + index
                }
                onClicked: ruleConfig.currentIndex = index
            }
            highlight: Rectangle {
                width: 180; height: 40
                color: "lightsteelblue"; radius: 5
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
                    name: "None"
                    index: 0
                }
                ListElement {
                    name: "Replace"
                    index: 1
                }
            }
        }
        
        StackLayout {
            id: ruleConfig
            currentIndex: 0

            Rectangle {
                color: "teal"

                Text {
                    text: "Select a rule type"
                }
            }
            Rules.Replace {}
        }
    }
}