import Vue from 'vue';
import Resource from 'vue-resource';

Vue.use(Resource);

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
    methods: {
        refreshFeatures(resource) {
            Vue.http.get('/api/Feature').then((response) => {
                this.features = response.data;
            });
        }
    }
});