import Vue from 'vue';
import Resource from 'vue-resource';
import { Tabs, Tab } from 'vue-tabs-component';

Vue.component('tabs', Tabs);
Vue.component('tab', Tab);

Vue.use(Resource);

//Build me using webpack --mode development in the noesbot.web folder

var featureapp = new Vue({
    el: '#features',
    created() {
        this.refreshFeatures();
    },
    data: {
        message: 'Hello Vue!',
        todos: [
            { text: 'Learn JavaScript' },
            { text: 'Learn Vue' },
            { text: 'Build something awesome' }
        ],
        features: []
    },
    components: { Tab, Tabs },
    methods: {
        refreshFeatures(resource) {
            Vue.http.get('/api/Feature').then((response) => {
                this.features = response.data;
            }).then(() => {
                // Hack, TODO fix/improve this
                var first = $(".tabs-component .tabs-component-tab a")[0];
                if (first) {
                    first.click();
                }
            });
        }
    }
});